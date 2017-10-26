﻿using UnityEngine;

public class BioAnimation : MonoBehaviour {

	public bool Inspect = false;

	public float TargetBlending = 0.25f;
	public float GaitTransition = 0.25f;
	public float TrajectoryCorrection = 0.75f;

	public Vector3 TargetDirection;
	public Vector3 TargetVelocity;

	public Transform Root;
	public Transform[] Joints = new Transform[0];

	public Controller Controller;
	public Character Character;
	public Trajectory Trajectory;
	public PFNN PFNN;

	private float Phase = 0f;
	private float UnitScale = 100f;

	private enum DrawingMode {Scene, Game};

	private Vector3[] Velocities;

	void Reset() {
		Controller = new Controller();
		Character = new Character(transform);
		Trajectory = new Trajectory();
		PFNN = new PFNN();
	}

	void Awake() {
		TargetDirection = new Vector3(transform.forward.x, 0f, transform.forward.z);
		TargetVelocity = Vector3.zero;
		Trajectory.Initialise(transform.position, TargetDirection);
		PFNN.Initialise();
		Velocities = new Vector3[Joints.Length];
	}

	void Start() {
		Utility.SetFPS(60);
	}

	void Update() {
		if(PFNN.Parameters == null) {
			return;
		}
		
		//Update Target Direction / Velocity
		TargetDirection = Vector3.Lerp(TargetDirection, Quaternion.AngleAxis(Controller.QueryTurn()*60f, Vector3.up) * Trajectory.GetRoot().GetDirection(), TargetBlending);
		TargetVelocity = Vector3.Lerp(TargetVelocity, (Quaternion.LookRotation(TargetDirection, Vector3.up) * Controller.QueryMove()).normalized, TargetBlending);

		//TODO: Update strafe etc.
		
		//Update Gait
		if(Vector3.Magnitude(TargetVelocity) < 0.1f) {
			float standAmount = 1.0f - Mathf.Clamp(Vector3.Magnitude(TargetVelocity) / 0.1f, 0.0f, 1.0f);
			Trajectory.GetRoot().Stand = Utility.Interpolate(Trajectory.GetRoot().Stand, standAmount, GaitTransition);
			Trajectory.GetRoot().Walk = Utility.Interpolate(Trajectory.GetRoot().Walk, 0f, GaitTransition);
			Trajectory.GetRoot().Jog = Utility.Interpolate(Trajectory.GetRoot().Jog, 0f, GaitTransition);
			Trajectory.GetRoot().Crouch = Utility.Interpolate(Trajectory.GetRoot().Crouch, Controller.QueryCrouch(), GaitTransition);
			//Trajectory.GetRoot().Jump = Utility.Interpolate(Trajectory.GetRoot().Jump, Controller.QueryJump(), GaitSmoothing);
			Trajectory.GetRoot().Bump = Utility.Interpolate(Trajectory.GetRoot().Bump, 0f, GaitTransition);
		} else {
			float standAmount = 1.0f - Mathf.Clamp(Vector3.Magnitude(TargetVelocity) / 0.1f, 0.0f, 1.0f);
			Trajectory.GetRoot().Stand = Utility.Interpolate(Trajectory.GetRoot().Stand, standAmount, GaitTransition);
			Trajectory.GetRoot().Walk = Utility.Interpolate(Trajectory.GetRoot().Walk, 1f-Controller.QueryJog(), GaitTransition);
			Trajectory.GetRoot().Jog = Utility.Interpolate(Trajectory.GetRoot().Jog, Controller.QueryJog(), GaitTransition);
			Trajectory.GetRoot().Crouch = Utility.Interpolate(Trajectory.GetRoot().Crouch, Controller.QueryCrouch(), GaitTransition);
			//Trajectory.GetRoot().Jump = Utility.Interpolate(Trajectory.GetRoot().Jump, Controller.QueryJump(), GaitSmoothing);
			Trajectory.GetRoot().Bump = Utility.Interpolate(Trajectory.GetRoot().Bump, 0f, GaitTransition);
		}
		//TODO: Update gait for jog, crouch, ...

		//Blend Trajectory Offset
		/*
		Vector3 positionOffset = transform.position - Trajectory.GetRoot().GetPosition();
		Quaternion rotationOffset = Quaternion.Inverse(Trajectory.GetRoot().GetRotation()) * transform.rotation;
		Trajectory.GetRoot().SetPosition(Trajectory.GetRoot().GetPosition() + positionOffset, false);
		Trajectory.GetRoot().SetDirection(rotationOffset * Trajectory.GetRoot().GetDirection());
		*/
		/*
		for(int i=Trajectory.GetRootPointIndex(); i<Trajectory.GetPointCount(); i++) {
			float factor = 1f - (i - Trajectory.GetRootPointIndex())/(Trajectory.GetRootPointIndex() - 1f);
			Trajectory.Points[i].SetPosition(Trajectory.Points[i].GetPosition() + factor*positionOffset, false);
		}
		*/

		//Predict Future Trajectory
		Vector3[] trajectory_positions_blend = new Vector3[Trajectory.GetPointCount()];
		trajectory_positions_blend[Trajectory.GetRootPointIndex()] = Trajectory.GetRoot().GetPosition();

		for(int i=Trajectory.GetRootPointIndex()+1; i<Trajectory.GetPointCount(); i++) {
			float bias_pos = 0.75f;
			float bias_dir = 1.25f;
			float scale_pos = (1.0f - Mathf.Pow(1.0f - ((float)(i - Trajectory.GetRootPointIndex()) / (Trajectory.GetRootPointIndex())), bias_pos));
			float scale_dir = (1.0f - Mathf.Pow(1.0f - ((float)(i - Trajectory.GetRootPointIndex()) / (Trajectory.GetRootPointIndex())), bias_dir));
			float vel_boost = 1f;

			float rescale = 1f / (Trajectory.GetPointCount() - (Trajectory.GetRootPointIndex() + 1f));
			trajectory_positions_blend[i] = trajectory_positions_blend[i-1] + Vector3.Lerp(
				Trajectory.Points[i].GetPosition() - Trajectory.Points[i-1].GetPosition(), 
				vel_boost * rescale * TargetVelocity,
				scale_pos);
				
			Trajectory.Points[i].SetDirection(Vector3.Lerp(Trajectory.Points[i].GetDirection(), TargetDirection, scale_dir));

			Trajectory.Points[i].Stand = Trajectory.GetRoot().Stand;
			Trajectory.Points[i].Walk = Trajectory.GetRoot().Walk;
			Trajectory.Points[i].Jog = Trajectory.GetRoot().Jog;
			Trajectory.Points[i].Crouch = Trajectory.GetRoot().Crouch;
			//Trajectory.Points[i].Jump = Trajectory.GetRoot().Jump;
			Trajectory.Points[i].Bump = Trajectory.GetRoot().Bump;
		}
		
		for(int i=Trajectory.GetRootPointIndex()+1; i<Trajectory.GetPointCount(); i++) {
			Trajectory.Points[i].SetPosition(trajectory_positions_blend[i], true);
		}

		//Post-Correct Trajectory
		CollisionChecks(Trajectory.GetRootPointIndex()+1);

		//Calculate Root
		Transformation currentRoot = new Transformation(Trajectory.GetRoot().GetPosition(), Trajectory.GetRoot().GetRotation());

		//Input Trajectory Positions / Directions
		for(int i=0; i<Trajectory.GetSampleCount(); i++) {
			Vector3 pos = Trajectory.Points[Trajectory.GetDensity()*i].GetPosition().RelativePositionTo(currentRoot);
			Vector3 dir = Trajectory.Points[Trajectory.GetDensity()*i].GetDirection().RelativeDirectionTo(currentRoot);
			PFNN.SetInput(Trajectory.GetSampleCount()*0 + i, UnitScale * pos.x);
			PFNN.SetInput(Trajectory.GetSampleCount()*1 + i, UnitScale * pos.z);
			PFNN.SetInput(Trajectory.GetSampleCount()*2 + i, dir.x);
			PFNN.SetInput(Trajectory.GetSampleCount()*3 + i, dir.z);
		}

		//Input Trajectory Gaits
		for (int i=0; i<Trajectory.GetSampleCount(); i++) {
			PFNN.SetInput(Trajectory.GetSampleCount()*4 + i, Trajectory.Points[Trajectory.GetDensity()*i].Stand);
			PFNN.SetInput(Trajectory.GetSampleCount()*5 + i, Trajectory.Points[Trajectory.GetDensity()*i].Walk);
			PFNN.SetInput(Trajectory.GetSampleCount()*6 + i, Trajectory.Points[Trajectory.GetDensity()*i].Jog);
			PFNN.SetInput(Trajectory.GetSampleCount()*7 + i, Trajectory.Points[Trajectory.GetDensity()*i].Crouch);
			PFNN.SetInput(Trajectory.GetSampleCount()*8 + i, Trajectory.Points[Trajectory.GetDensity()*i].Jump);
			PFNN.SetInput(Trajectory.GetSampleCount()*9 + i, Trajectory.Points[Trajectory.GetDensity()*i].Bump);
		}

		//Input Previous Bone Positions / Velocities
		Transformation previousRoot = new Transformation(Trajectory.GetPrevious().GetPosition(), Trajectory.GetPrevious().GetRotation());
		for(int i=0; i<Joints.Length; i++) {
			int o = 10*Trajectory.GetSampleCount();
			Vector3 pos = Joints[i].position.RelativePositionTo(previousRoot);
			Vector3 vel = Velocities[i].RelativeDirectionTo(previousRoot);
			PFNN.SetInput(o + Joints.Length*3*0 + i*3+0, UnitScale * pos.x);
			PFNN.SetInput(o + Joints.Length*3*0 + i*3+1, UnitScale * pos.y);
			PFNN.SetInput(o + Joints.Length*3*0 + i*3+2, UnitScale * pos.z);
			PFNN.SetInput(o + Joints.Length*3*1 + i*3+0, UnitScale * vel.x);
			PFNN.SetInput(o + Joints.Length*3*1 + i*3+1, UnitScale * vel.y);
			PFNN.SetInput(o + Joints.Length*3*1 + i*3+2, UnitScale * vel.z);
		}

		//Input Trajectory Heights
		for(int i=0; i<Trajectory.GetSampleCount(); i++) {
			int o = 10*Trajectory.GetSampleCount() + Joints.Length*3*2;
			PFNN.SetInput(o + Trajectory.GetSampleCount()*0 + i, UnitScale * (Trajectory.Points[Trajectory.GetDensity()*i].Project(Trajectory.Width/2f).y - currentRoot.Position.y));
			PFNN.SetInput(o + Trajectory.GetSampleCount()*1 + i, UnitScale * (Trajectory.Points[Trajectory.GetDensity()*i].GetHeight() - currentRoot.Position.y));
			PFNN.SetInput(o + Trajectory.GetSampleCount()*2 + i, UnitScale * (Trajectory.Points[Trajectory.GetDensity()*i].Project(-Trajectory.Width/2f).y - currentRoot.Position.y));
		}

		//Predict
		PFNN.Predict(Phase);

		//Update Past Trajectory
		for(int i=0; i<Trajectory.GetRootPointIndex(); i++) {
			Trajectory.Points[i].SetPosition(Trajectory.Points[i+1].GetPosition(), true);
			Trajectory.Points[i].SetDirection(Trajectory.Points[i+1].GetDirection());
			Trajectory.Points[i].Stand = Trajectory.Points[i+1].Stand;
			Trajectory.Points[i].Walk = Trajectory.Points[i+1].Walk;
			Trajectory.Points[i].Jog = Trajectory.Points[i+1].Jog;
			Trajectory.Points[i].Crouch = Trajectory.Points[i+1].Crouch;
			//Trajectory.Points[i].Jump = Trajectory.Points[i+1].Jump;
			Trajectory.Points[i].Bump = Trajectory.Points[i+1].Bump;
		}

		//Update Current Trajectory
		float stand_amount = Mathf.Pow(1.0f-Trajectory.GetRoot().Stand, 0.25f);
		Trajectory.GetRoot().SetPosition((stand_amount * new Vector3(PFNN.GetOutput(0) / UnitScale, 0f, PFNN.GetOutput(1) / UnitScale)).RelativePositionFrom(currentRoot), true);
		Trajectory.GetRoot().SetDirection(Quaternion.AngleAxis(stand_amount * Mathf.Rad2Deg * (-PFNN.GetOutput(2)), Vector3.up) * Trajectory.GetRoot().GetDirection());
		Transformation newRoot = new Transformation(Trajectory.GetRoot().GetPosition(), Trajectory.GetRoot().GetRotation());

		for(int i=Trajectory.GetRootPointIndex()+1; i<Trajectory.GetPointCount(); i++) {
			Trajectory.Points[i].SetPosition(Trajectory.Points[i].GetPosition() + (stand_amount * new Vector3(PFNN.GetOutput(0) / UnitScale, 0f, PFNN.GetOutput(1) / UnitScale)).RelativeDirectionFrom(newRoot), true);
		}
		
		//Update Future Trajectory
		for(int i=Trajectory.GetRootPointIndex()+1; i<Trajectory.GetPointCount(); i++) {
			int w = Trajectory.GetRootSampleIndex();
			float m = Mathf.Repeat(((float)i - (float)Trajectory.GetRootPointIndex()) / (float)Trajectory.GetDensity(), 1.0f);
			float posX = (1-m) * PFNN.GetOutput(8+(w*0)+(i/Trajectory.GetDensity())-w) + m * PFNN.GetOutput(8+(w*0)+(i/Trajectory.GetDensity())-w+1);
			float posZ = (1-m) * PFNN.GetOutput(8+(w*1)+(i/Trajectory.GetDensity())-w) + m * PFNN.GetOutput(8+(w*1)+(i/Trajectory.GetDensity())-w+1);
			float dirX = (1-m) * PFNN.GetOutput(8+(w*2)+(i/Trajectory.GetDensity())-w) + m * PFNN.GetOutput(8+(w*2)+(i/Trajectory.GetDensity())-w+1);
			float dirZ = (1-m) * PFNN.GetOutput(8+(w*3)+(i/Trajectory.GetDensity())-w) + m * PFNN.GetOutput(8+(w*3)+(i/Trajectory.GetDensity())-w+1);
			Trajectory.Points[i].SetPosition(
				Utility.Interpolate(
					Trajectory.Points[i].GetPosition(),
					new Vector3(posX / UnitScale, 0f, posZ / UnitScale).RelativePositionFrom(newRoot),
					TrajectoryCorrection
					),
					true
				);
			Trajectory.Points[i].SetDirection(
				Utility.Interpolate(
					Trajectory.Points[i].GetDirection(),
					new Vector3(dirX, 0f, dirZ).normalized.RelativeDirectionFrom(newRoot),
					TrajectoryCorrection
					)
				);
		}

		//Post-Correct Trajectory
		CollisionChecks(Trajectory.GetRootPointIndex());
		
		//Update Posture
		//TODO: Do not override...
		Root.OverridePosition(newRoot.Position);
		Root.OverrideRotation(newRoot.Rotation);
		int opos = 8 + 4*Trajectory.GetRootSampleIndex() + Joints.Length*3*0;
		int ovel = 8 + 4*Trajectory.GetRootSampleIndex() + Joints.Length*3*1;
		for(int i=0; i<Joints.Length; i++) {			
			Vector3 position = new Vector3(PFNN.GetOutput(opos+i*3+0), PFNN.GetOutput(opos+i*3+1), PFNN.GetOutput(opos+i*3+2)) / UnitScale;
			Vector3 velocity = new Vector3(PFNN.GetOutput(ovel+i*3+0), PFNN.GetOutput(ovel+i*3+1), PFNN.GetOutput(ovel+i*3+2)) / UnitScale;
			Joints[i].OverridePosition(Vector3.Lerp(Joints[i].position.RelativePositionTo(currentRoot) + velocity, position, 0.5f).RelativePositionFrom(currentRoot));
			Joints[i].OverrideRotation(Quaternion.identity); //TODO: Compute mesh rotations
			Velocities[i] = velocity.RelativeDirectionFrom(currentRoot);
		}
		//Character.ForwardKinematics();

		/* Update Phase */
		Phase = Mathf.Repeat(Phase + (stand_amount * 0.9f + 0.1f) * PFNN.GetOutput(3) * 2f*Mathf.PI, 2f*Mathf.PI);

		//PFNN.Finish();
	}

	private void CollisionChecks(int start) {
		for(int i=start; i<Trajectory.GetPointCount(); i++) {
			float safety = 0.5f;
			Vector3 previousPos = Trajectory.Points[i-1].GetPosition();
			Vector3 currentPos = Trajectory.Points[i].GetPosition();
			Vector3 testPos = previousPos + safety*(currentPos-previousPos).normalized;
			Vector3 projectedPos = Utility.ProjectCollision(previousPos, testPos, LayerMask.GetMask("Obstacles"));
			if(testPos != projectedPos) {
				Vector3 correctedPos = testPos + safety * (previousPos-testPos).normalized;
				Trajectory.Points[i].SetPosition(correctedPos, true);
			}
		}
	}

	public void SetRoot(Transform root) {
		Root = root;
	}

	public void SetJoint(int index, Transform t) {
		if(index < 0 || index >= Joints.Length) {
			return;
		}
		Joints[index] = t;
	}

	public void SetJointCount(int count) {
		count = Mathf.Max(0, count);
		if(Joints.Length != count) {
			System.Array.Resize(ref Joints, count);
		}
	}

	void OnRenderObject() {
		Trajectory.Draw();
		Color transparentTargetDirection = new Color(Utility.Red.r, Utility.Red.g, Utility.Red.b, 0.75f);
		Color transparentTargetVelocity = new Color(Utility.Green.r, Utility.Green.g, Utility.Green.b, 0.75f);
		UnityGL.DrawLine(Trajectory.GetRoot().GetPosition(), Trajectory.GetRoot().GetPosition() + TargetDirection, 0.05f, 0f, transparentTargetDirection);
		UnityGL.DrawLine(Trajectory.GetRoot().GetPosition(), Trajectory.GetRoot().GetPosition() + TargetVelocity, 0.05f, 0f, transparentTargetVelocity);
		Character.Draw();
		for(int i=0; i<Character.Bones.Length; i++) {
			Character.Bone bone = Character.Bones[i];
			if(bone.Draw) {
				UnityGL.DrawArrow(
					bone.Transform.position,
					bone.Transform.position + 10f*bone.GetVelocity(),
					0.75f,
					0.0075f,
					0.05f,
					new Color(0f, 1f, 0f, 0.5f)
				);
			}
		}
	}

	void OnDrawGizmos() {
		if(!Application.isPlaying) {
			OnRenderObject();
		}
	}
	
}
