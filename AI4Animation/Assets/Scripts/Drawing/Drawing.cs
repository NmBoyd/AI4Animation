﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Drawing {

	private static int Resolution = 30;

	private static Mesh Initialised;

	private static bool Active;

	private static Material GLMaterial;
	private static Material MeshMaterial;

	private static float GUIOffset = 0.001f;

	private static Camera Camera;
	private static Vector3 ViewPosition;
	private static Quaternion ViewRotation;

	private static PROGRAM Program = PROGRAM.NONE;
	private enum PROGRAM {NONE, LINES, LINE_STRIP, TRIANGLES, TRIANGLE_STRIP, QUADS};

	private static Mesh CircleMesh;
	private static Mesh QuadMesh;
	private static Mesh CubeMesh;
	private static Mesh SphereMesh;
	private static Mesh CylinderMesh;
	private static Mesh CapsuleMesh;
	public static Mesh BoneMesh;

	private static Vector3[] CircleWire;
	private static Vector3[] QuadWire;
	private static Vector3[] CubeWire;
	private static Vector3[] SphereWire;
	private static Vector3[] CylinderWire;
	private static Vector3[] CapsuleWire;
	private static Vector3[] BoneWire;

	//------------------------------------------------------------------------------------------
	//CONTROL FUNCTIONS
	//------------------------------------------------------------------------------------------
	public static void Begin() {
		if(Active) {
			Debug.Log("Drawing is still active. Call 'End' to stop.");
		} else {
			Initialise();
			Camera = GetCamera();
			ViewPosition = Camera.transform.position;
			ViewRotation = Camera.transform.rotation;
			Active = true;
		}
	}

	public static void End() {
		if(Active) {
			SetProgram(PROGRAM.NONE);
			Camera = null;
			ViewPosition = Vector3.zero;
			ViewRotation = Quaternion.identity;
			Active = false;
		} else {
			Debug.Log("Drawing is not active. Call 'Begin()' first.");
		}
	}

	//------------------------------------------------------------------------------------------
	//2D SCENE DRAWING FUNCTIONS
	//------------------------------------------------------------------------------------------
	public static void DrawLine(Vector3 start, Vector3 end, Color color) {
		if(Return()) {return;};
		SetProgram(PROGRAM.LINES);
		GL.Color(color);
		GL.Vertex(start);
		GL.Vertex(end);
	}

    public static void DrawLine(Vector3 start, Vector3 end, float width, Color color) {
		if(Return()) {return;};
		SetProgram(PROGRAM.QUADS);
		GL.Color(color);
		Vector3 dir = (end-start).normalized;
		Vector3 orthoStart = width/2f * (Quaternion.AngleAxis(90f, (start - ViewPosition)) * dir);
		Vector3 orthoEnd = width/2f * (Quaternion.AngleAxis(90f, (end - ViewPosition)) * dir);
		GL.Vertex(end+orthoEnd);
		GL.Vertex(end-orthoEnd);
		GL.Vertex(start-orthoStart);
		GL.Vertex(start+orthoStart);
    }

    public static void DrawLine(Vector3 start, Vector3 end, float startWidth, float endWidth, Color color) {
		if(Return()) {return;};
		SetProgram(PROGRAM.QUADS);
		GL.Color(color);
		Vector3 dir = (end-start).normalized;
		Vector3 orthoStart = startWidth/2f * (Quaternion.AngleAxis(90f, (start - ViewPosition)) * dir);
		Vector3 orthoEnd = endWidth/2f * (Quaternion.AngleAxis(90f, (end - ViewPosition)) * dir);
		GL.Vertex(end+orthoEnd);
		GL.Vertex(end-orthoEnd);
		GL.Vertex(start-orthoStart);
		GL.Vertex(start+orthoStart);
    }

	public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color) {
		if(Return()) {return;};
		SetProgram(PROGRAM.TRIANGLES);
		GL.Color(color);
        GL.Vertex(b);
		GL.Vertex(a);
		GL.Vertex(c);
	}

	public static void DrawCircle(Vector3 position, float size, Color color) {
		DrawMesh(CircleMesh, position, ViewRotation, size*Vector3.one, color);
	}

	public static void DrawCircle(Vector3 position, Quaternion rotation, float size, Color color) {
		DrawMesh(CircleMesh, position, rotation, size*Vector3.one, color);
	}

	public static void DrawWireCircle(Vector3 position, float size, Color color) {
		DrawWireLineStrip(CircleWire, position, ViewRotation, size*Vector3.one, color);
	}

	public static void DrawWireCircle(Vector3 position, Quaternion rotation, float size, Color color) {
		DrawWireLineStrip(CircleWire, position, rotation, size*Vector3.one, color);
	}

	public static void DrawWiredCircle(Vector3 position, float size, Color circleColor, Color wireColor) {
		DrawCircle(position, size, circleColor);
		DrawWireCircle(position, size, wireColor);
	}

	public static void DrawWiredCircle(Vector3 position, Quaternion rotation, float size, Color circleColor, Color wireColor) {
		DrawCircle(position, rotation, size, circleColor);
		DrawWireCircle(position, rotation, size, wireColor);
	}

	public static void DrawArrow(Vector3 start, Vector3 end, float tipPivot, float shaftWidth, float tipWidth, Color color) {
		tipPivot = Mathf.Clamp(tipPivot, 0f, 1f);
		Vector3 pivot = start + tipPivot * (end-start);
		DrawLine(start, pivot, shaftWidth, color);
		DrawLine(pivot, end, tipWidth, 0f, color);
	}

	public static void DrawArrow(Vector3 start, Vector3 end, float tipPivot, float shaftWidth, float tipWidth, Color shaftColor, Color tipColor) {
		tipPivot = Mathf.Clamp(tipPivot, 0f, 1f);
		Vector3 pivot = start + tipPivot * (end-start);
		DrawLine(start, pivot, shaftWidth, shaftColor);
		DrawLine(pivot, end, tipWidth, 0f, tipColor);
	}

	//------------------------------------------------------------------------------------------
	//3D SCENE DRAWING FUNCTIONS
	//------------------------------------------------------------------------------------------
	public static void DrawQuad(Vector3 position, Quaternion rotation, float width, float height, Color color) {
		DrawMesh(QuadMesh, position, rotation, new Vector3(width, height, 1f), color);
	}

	public static void DrawWireQuad(Vector3 position, Quaternion rotation, float width, float height, Color color) {
		DrawWireLineStrip(QuadWire, position, rotation, new Vector3(width, height, 1f), color);
	}

	public static void DrawWiredQuad(Vector3 position, Quaternion rotation, float width, float height, Color quadColor, Color wireColor) {
		DrawQuad(position, rotation, width, height, quadColor);
		DrawWireQuad(position, rotation, width, height, wireColor);
	}

	public static void DrawCube(Vector3 position, Quaternion rotation, float size, Color color) {
		DrawMesh(CubeMesh, position, rotation, size*Vector3.one, color);
	}

	public static void DrawWireCube(Vector3 position, Quaternion rotation, float size, Color color) {
		DrawWireLines(CubeWire, position, rotation, size*Vector3.one, color);
	}

	public static void DrawWiredCube(Vector3 position, Quaternion rotation, float size, Color cubeColor, Color wireColor) {
		DrawCube(position, rotation, size, cubeColor);
		DrawWireCube(position, rotation, size, wireColor);
	}

	public static void DrawCuboid(Vector3 position, Quaternion rotation, Vector3 size, Color color) {
		DrawMesh(CubeMesh, position, rotation, size, color);
	}

	public static void DrawWireCuboid(Vector3 position, Quaternion rotation, Vector3 size, Color color) {
		DrawWireLines(CubeWire, position, rotation, size, color);
	}

	public static void DrawWiredCuboid(Vector3 position, Quaternion rotation, Vector3 size, Color cuboidColor, Color wireColor) {
		DrawCuboid(position, rotation, size, cuboidColor);
		DrawWireCuboid(position, rotation, size, wireColor);
	}

	public static void DrawSphere(Vector3 position, float size, Color color) {
		DrawMesh(SphereMesh, position, Quaternion.identity, size*Vector3.one, color);
	}

	public static void DrawWireSphere(Vector3 position, float size, Color color) {
		DrawWireLines(SphereWire, position, Quaternion.identity, size*Vector3.one, color);
	}

	public static void DrawWiredSphere(Vector3 position, float size, Color sphereColor, Color wireColor) {
		DrawSphere(position, size, sphereColor);
		DrawWireSphere(position, size, wireColor);
	}

	public static void DrawCylinder(Vector3 position, Quaternion rotation, float width, float height, Color color) {
		DrawMesh(CylinderMesh, position, rotation, new Vector3(width, height/2f, width), color);
	}

	public static void DrawWireCylinder(Vector3 position, Quaternion rotation, float width, float height, Color color) {
		DrawWireLines(CylinderWire, position, rotation, new Vector3(width, height/2f, width), color);
	}

	public static void DrawWiredCylinder(Vector3 position, Quaternion rotation, float width, float height, Color cylinderColor, Color wireColor) {
		DrawCylinder(position, rotation, width, height, cylinderColor);
		DrawWireCylinder(position, rotation, width, height, wireColor);
	}

	public static void DrawCapsule(Vector3 position, Quaternion rotation, float width, float height, Color color) {
		DrawMesh(CapsuleMesh, position, rotation, new Vector3(width, height/2f, width), color);
	}

	public static void DrawWireCapsule(Vector3 position, Quaternion rotation, float width, float height, Color color) {
		DrawWireLines(CapsuleWire, position, rotation, new Vector3(width, height/2f, width), color);
	}

	public static void DrawWiredCapsule(Vector3 position, Quaternion rotation, float width, float height, Color capsuleColor, Color wireColor) {
		DrawCapsule(position, rotation, width, height, capsuleColor);
		DrawWireCapsule(position, rotation, width, height, wireColor);
	}

	public static void DrawBone(Vector3 position, Quaternion rotation, float width, float length, Color color) {
		DrawMesh(BoneMesh, position, rotation, new Vector3(width, width, length), color);
	}

	public static void DrawWireBone(Vector3 position, Quaternion rotation, float width, float length, Color color) {
		DrawWireLines(BoneWire, position, rotation, new Vector3(width, width, length), color);
	}

	public static void DrawWiredBone(Vector3 position, Quaternion rotation, float width, float length, Color boneColor, Color wireColor) {
		DrawBone(position, rotation, width, length, boneColor);
		DrawWireBone(position, rotation, width, length, wireColor);
	}

	public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, Color color) {
		if(Return()) {return;}
		SetProgram(PROGRAM.NONE);
		MeshMaterial.color = color;
		MeshMaterial.SetPass(0);
		Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(position, rotation, scale));
	}

	//------------------------------------------------------------------------------------------
	//GUI DRAWING FUNCTIONS
	//------------------------------------------------------------------------------------------
	public static void DrawGUILine(Vector2 start, Vector2 end, Color color) {
		if(Camera != Camera.main) {return;}
		if(Return()) {return;}
		SetProgram(PROGRAM.LINES);
		GL.Color(color);
		start.x *= Screen.width;
		start.y *= Screen.height;
		end.x *= Screen.width;
		end.y *= Screen.height;
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(start.x, start.y, Camera.nearClipPlane + GUIOffset)));
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(end.x, end.y, Camera.nearClipPlane + GUIOffset)));
	}

    public static void DrawGUILine(Vector2 start, Vector2 end, float width, Color color) {
		if(Camera != Camera.main) {return;}
		if(Return()) {return;}
		SetProgram(PROGRAM.QUADS);
		GL.Color(color);
		start.x *= Screen.width;
		start.y *= Screen.height;
		end.x *= Screen.width;
		end.y *= Screen.height;
		width *= Screen.width;
		Vector3 p1 = new Vector3(start.x, start.y, Camera.nearClipPlane + GUIOffset);
		Vector3 p2 = new Vector3(end.x, end.y, Camera.nearClipPlane + GUIOffset);
		Vector3 dir = end-start;
		Vector3 ortho = width/2f * (Quaternion.AngleAxis(90f, Vector3.forward) * dir).normalized;
        GL.Vertex(Camera.ScreenToWorldPoint(p1-ortho));
		GL.Vertex(Camera.ScreenToWorldPoint(p1+ortho));
		GL.Vertex(Camera.ScreenToWorldPoint(p2+ortho));
		GL.Vertex(Camera.ScreenToWorldPoint(p2-ortho));
    }

	public static void DrawGUIRectangle(Vector2 center, float width, float height, Color color) {
		if(Camera != Camera.main) {return;}
		if(Return()) {return;}
		SetProgram(PROGRAM.QUADS);
		GL.Color(color);
		center.x *= Screen.width;
		center.y *= Screen.height;
		width *= Screen.width;
		height *= Screen.height;
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(center.x+width/2f, center.y-height/2f, Camera.nearClipPlane + GUIOffset)));
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(center.x-width/2f, center.y-height/2f, Camera.nearClipPlane + GUIOffset)));
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(center.x+-width/2f, center.y+height/2f, Camera.nearClipPlane + GUIOffset)));
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(center.x+width/2f, center.y+height/2f, Camera.nearClipPlane + GUIOffset)));
	}

	public static void DrawGUITriangle(Vector2 a, Vector2 b, Vector2 c, Color color) {
		if(Camera != Camera.main) {return;}
		if(Return()) {return;}
		SetProgram(PROGRAM.TRIANGLES);
		GL.Color(color);
		a.x *= Screen.width;
		a.y *= Screen.height;
		b.x *= Screen.width;
		b.y *= Screen.height;
		c.x *= Screen.width;
		c.y *= Screen.height;
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(a.x, a.y, Camera.nearClipPlane + GUIOffset)));
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(b.x, b.y, Camera.nearClipPlane + GUIOffset)));
		GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(c.x, c.y, Camera.nearClipPlane + GUIOffset)));
	}

	public static void DrawGUICircle(Vector2 center, float size, Color color) {
		if(Camera != Camera.main) {return;}
		if(Return()) {return;}
		SetProgram(PROGRAM.TRIANGLE_STRIP);
		GL.Color(color);
		center.x *= Screen.width;
		center.y *= Screen.height;
		for(int i=0; i<CircleWire.Length; i++) {
			GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(center.x + size*CircleWire[i].x*Screen.width, center.y + size*CircleWire[i].y*Screen.width, Camera.nearClipPlane + GUIOffset)));
			GL.Vertex(Camera.ScreenToWorldPoint(new Vector3(center.x, center.y, Camera.nearClipPlane + GUIOffset)));
		}
	}

	//------------------------------------------------------------------------------------------
	//UTILITY FUNCTIONS
	//------------------------------------------------------------------------------------------
	private static bool Return() {
		if(!Active) {
			Debug.Log("Drawing is not active. Call 'Begin()' first.");
		}
		return !Active;
	}

	static void Initialise() {
		if(Initialised != null) {
			return;
		}

		Resources.UnloadUnusedAssets();

		GLMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
		GLMaterial.hideFlags = HideFlags.HideAndDontSave;
		GLMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
		GLMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		GLMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
		GLMaterial.SetInt("_ZWrite", 1);
		GLMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

		MeshMaterial = new Material(Shader.Find("UnityGL"));
		MeshMaterial.hideFlags = HideFlags.HideAndDontSave;
		MeshMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
		MeshMaterial.SetInt("_ZWrite", 1);
		MeshMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
		MeshMaterial.SetFloat("_Power", 0.25f);

		//Meshes
		CircleMesh = CreateCircleMesh(Resolution);
		QuadMesh = GetPrimitiveMesh(PrimitiveType.Quad);
		CubeMesh = GetPrimitiveMesh(PrimitiveType.Cube);
		SphereMesh = GetPrimitiveMesh(PrimitiveType.Sphere);
		CylinderMesh = GetPrimitiveMesh(PrimitiveType.Cylinder);
		CapsuleMesh = GetPrimitiveMesh(PrimitiveType.Capsule);
		BoneMesh = CreateBoneMesh();
		//

		//Wires
		CircleWire = CreateCircleWire(Resolution);
		QuadWire = CreateQuadWire();
		CubeWire = CreateCubeWire();
		SphereWire = CreateSphereWire(Resolution);
		CylinderWire = CreateCylinderWire(Resolution);
		CapsuleWire = CreateCapsuleWire(Resolution);
		BoneWire = CreateBoneWire();
		//
		
		Initialised = new Mesh();
	}

	private static void SetProgram(PROGRAM program) {
		if(Program != program) {
			Program = program;
			GL.End();
			if(Program != PROGRAM.NONE) {
				GLMaterial.SetPass(0);
				switch(Program) {
					case PROGRAM.LINES:
					GL.Begin(GL.LINES);
					break;
					case PROGRAM.LINE_STRIP:
					GL.Begin(GL.LINE_STRIP);
					break;
					case PROGRAM.TRIANGLES:
					GL.Begin(GL.TRIANGLES);
					break;
					case PROGRAM.TRIANGLE_STRIP:
					GL.Begin(GL.TRIANGLE_STRIP);
					break;
					case PROGRAM.QUADS:
					GL.Begin(GL.QUADS);
					break;
				}
			}
		}
	}

	private static void DrawWireLines(Vector3[] points, Vector3 position, Quaternion rotation, Vector3 scale, Color color) {
		if(Return()) {return;};
		SetProgram(PROGRAM.LINES);
		GL.Color(color);
		for(int i=0; i<points.Length; i+=2) {
			GL.Vertex(position + rotation * Vector3.Scale(scale, points[i]));
			GL.Vertex(position + rotation * Vector3.Scale(scale, points[i+1]));
		}
	}

	private static void DrawWireLineStrip(Vector3[] points, Vector3 position, Quaternion rotation, Vector3 scale, Color color) {
		if(Return()) {return;};
		SetProgram(PROGRAM.LINE_STRIP);
		GL.Color(color);
		for(int i=0; i<points.Length; i++) {
			GL.Vertex(position + rotation * Vector3.Scale(scale, points[i]));
		}
	}

	private static Camera GetCamera() {
		if(Camera.current != null) {
			return Camera.current;
		} else {
			return Camera.main;
		}
	}

	private static Mesh GetPrimitiveMesh(PrimitiveType type) {
		GameObject gameObject = GameObject.CreatePrimitive(type);
		gameObject.hideFlags = HideFlags.HideInHierarchy;
		gameObject.GetComponent<MeshRenderer>().enabled = false;
		Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		if(Application.isPlaying) {
			GameObject.Destroy(gameObject);
		} else {
			GameObject.DestroyImmediate(gameObject);
		}
		return mesh;
	}
	
	private static Mesh CreateCircleMesh(int resolution) {
		float step = 360.0f / (float)resolution;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		Quaternion quaternion = Quaternion.Euler(0.0f, 0.0f, step);
		vertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
		vertices.Add(new Vector3(0.0f, 0.5f, 0.0f));
		vertices.Add(quaternion * vertices[1]);
		triangles.Add(1);
		triangles.Add(0);
		triangles.Add(2);
		for(int i=0; i<resolution-1; i++) {
			triangles.Add(vertices.Count - 1);
			triangles.Add(0);
			triangles.Add(vertices.Count);
			vertices.Add(quaternion * vertices[vertices.Count - 1]);
		}
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();     
		return mesh;
	}

	private static Mesh CreateBoneMesh() {
		float size = 1f/7f;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		vertices.Add(new Vector3(-size, -size, 0.200f));
		vertices.Add(new Vector3(-size, size, 0.200f));
		vertices.Add(new Vector3(0.000f, 0.000f, 0.000f));
		vertices.Add(new Vector3(size, size, 0.200f));
		vertices.Add(new Vector3(0.000f, 0.000f, 1.000f));
		vertices.Add(new Vector3(size, -size, 0.200f));
		vertices.Add(new Vector3(-size, size, 0.200f));
		vertices.Add(new Vector3(-size, -size, 0.200f));
		vertices.Add(new Vector3(0.000f, 0.000f, 1.000f));
		vertices.Add(new Vector3(size, -size, 0.200f));
		vertices.Add(new Vector3(0.000f, 0.000f, 1.000f));
		vertices.Add(new Vector3(-size, -size, 0.200f));
		vertices.Add(new Vector3(size, size, 0.200f));
		vertices.Add(new Vector3(-size, size, 0.200f));
		vertices.Add(new Vector3(0.000f, 0.000f, 1.000f));
		vertices.Add(new Vector3(size, size, 0.200f));
		vertices.Add(new Vector3(size, -size, 0.200f));
		vertices.Add(new Vector3(0.000f, 0.000f, 0.000f));
		vertices.Add(new Vector3(size, size, 0.200f));
		vertices.Add(new Vector3(0.000f, 0.000f, 0.000f));
		vertices.Add(new Vector3(-size, size, 0.200f));
		vertices.Add(new Vector3(size, -size, 0.200f));
		vertices.Add(new Vector3(-size, -size, 0.200f));
		vertices.Add(new Vector3(0.000f, 0.000f, 0.000f));
		for(int i=0; i<24; i++) {
			triangles.Add(i);
		}
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		return mesh;
	}

	private static Vector3[] CreateCircleWire(int resolution) {
		List<Vector3> points = new List<Vector3>();
		float step = 360.0f / (float)resolution;
		for(int i=0; i<resolution; i++) {
			points.Add(Quaternion.Euler(0f, 0f, i*step) * new Vector3(0f, 0.5f, 0f));
			points.Add(Quaternion.Euler(0f, 0f, (i+1)*step) * new Vector3(0f, 0.5f, 0f));
		}
		return points.ToArray();
	}

	private static Vector3[] CreateQuadWire() {
		List<Vector3> points = new List<Vector3>();
		points.Add(new Vector3(-0.5f, -0.5f, 0f));
		points.Add(new Vector3(0.5f, -0.5f, 0f));
		points.Add(new Vector3(0.5f, 0.5f, 0f));
		points.Add(new Vector3(-0.5f, 0.5f, 0f));
		points.Add(new Vector3(-0.5f, -0.5f, 0f));
		return points.ToArray();
	}

	private static Vector3[] CreateCubeWire() {
		float size = 1f;
		Vector3 A = new Vector3(-size/2f, -size/2f, -size/2f);
		Vector3 B = new Vector3(size/2f, -size/2f, -size/2f);
		Vector3 C = new Vector3(-size/2f, -size/2f, size/2f);
		Vector3 D = new Vector3(size/2f, -size/2f, size/2f);
		Vector3 p1 = A; Vector3 p2 = B;
		Vector3 p3 = C; Vector3 p4 = D;
		Vector3 p5 = -D; Vector3 p6 = -C;
		Vector3 p7 = -B; Vector3 p8 = -A;

		List<Vector3> points = new List<Vector3>();
		points.Add(p1); points.Add(p2);
		points.Add(p2); points.Add(p4);
		points.Add(p4); points.Add(p3);
		points.Add(p3); points.Add(p1);
		
		points.Add(p5); points.Add(p6);
		points.Add(p6); points.Add(p8);
		points.Add(p5); points.Add(p7);
		points.Add(p7); points.Add(p8);

		points.Add(p1); points.Add(p5);
		points.Add(p2); points.Add(p6);
		points.Add(p3); points.Add(p7);
		points.Add(p4); points.Add(p8);
		return points.ToArray();
	}

	private static Vector3[] CreateSphereWire(int resolution) {
		List<Vector3> points = new List<Vector3>();
		float step = 360.0f / (float)resolution;
		for(int i=0; i<resolution; i++) {
			points.Add(Quaternion.Euler(0f, 0f, i*step) * new Vector3(0f, 0.5f, 0f));
			points.Add(Quaternion.Euler(0f, 0f, (i+1)*step) * new Vector3(0f, 0.5f, 0f));
		}
		for(int i=0; i<resolution; i++) {
			points.Add(Quaternion.Euler(0f, i*step, 0f) * new Vector3(0f, 0f, 0.5f));
			points.Add(Quaternion.Euler(0f, (i+1)*step, 0f) * new Vector3(0f, 0f, 0.5f));
		}
		for(int i=0; i<resolution; i++) {
			points.Add(Quaternion.Euler(i*step, 0f, i*step) * new Vector3(0f, 0f, 0.5f));
			points.Add(Quaternion.Euler((i+1)*step, 0f, (i+1)*step) * new Vector3(0f, 0f, 0.5f));
		}
		return points.ToArray();
	}

	private static Vector3[] CreateCylinderWire(int resolution) {
		List<Vector3> points = new List<Vector3>();
		float step = 360.0f / (float)resolution;
		for(int i=0; i<resolution; i++) {
			points.Add(Quaternion.Euler(0f, i*step, 0f) * new Vector3(0f, 0f, 0.5f) + new Vector3(0f, 1f, 0f));
			points.Add(Quaternion.Euler(0f, (i+1)*step, 0f) * new Vector3(0f, 0f, 0.5f) + new Vector3(0f, 1f, 0f));
		}
		for(int i=0; i<resolution; i++) {
			points.Add(Quaternion.Euler(0f, i*step, 0f) * new Vector3(0f, 0f, 0.5f) - new Vector3(0f, 1f, 0f));
			points.Add(Quaternion.Euler(0f, (i+1)*step, 0f) * new Vector3(0f, 0f, 0.5f) - new Vector3(0f, 1f, 0f));
		}
		points.Add(new Vector3(0f, -1f, -0.5f));
		points.Add(new Vector3(0f, 1f, -0.5f));
		points.Add(new Vector3(0f, -1f, 0.5f));
		points.Add(new Vector3(0f, 1f, 0.5f));
		points.Add(new Vector3(-0.5f, -1f, 0f));
		points.Add(new Vector3(-0.5f, 1f, 0f));
		points.Add(new Vector3(0.5f, -1f, 0f));
		points.Add(new Vector3(0.5f, 1f, 0));
		return points.ToArray();
	}

	private static Vector3[] CreateCapsuleWire(int resolution) {
		List<Vector3> points = new List<Vector3>();
		float step = 360.0f / (float)resolution;
		for(int i=-resolution/4-1; i<=resolution/4; i++) {
			points.Add(Quaternion.Euler(0f, 0f, i*step) * new Vector3(0f, 0.5f, 0f) + new Vector3(0f, 0.5f, 0f));
			points.Add(Quaternion.Euler(0f, 0f, (i+1)*step) * new Vector3(0f, 0.5f, 0f) + new Vector3(0f, 0.5f, 0f));
		}
		for(int i=resolution/2; i<resolution; i++) {
			points.Add(Quaternion.Euler(i*step, 0f, i*step) * new Vector3(0f, 0f, 0.5f) + new Vector3(0f, 0.5f, 0f));
			points.Add(Quaternion.Euler((i+1)*step, 0f, (i+1)*step) * new Vector3(0f, 0f, 0.5f) + new Vector3(0f, 0.5f, 0f));
		}
		for(int i=-resolution/4-1; i<=resolution/4; i++) {
			points.Add(Quaternion.Euler(0f, 0f, i*step) * new Vector3(0f, -0.5f, 0f) + new Vector3(0f, -0.5f, 0f));
			points.Add(Quaternion.Euler(0f, 0f, (i+1)*step) * new Vector3(0f, -0.5f, 0f) + new Vector3(0f, -0.5f, 0f));
		}
		for(int i=resolution/2; i<resolution; i++) {
			points.Add(Quaternion.Euler(i*step, 0f, i*step) * new Vector3(0f, 0f, -0.5f) + new Vector3(0f, -0.5f, 0f));
			points.Add(Quaternion.Euler((i+1)*step, 0f, (i+1)*step) * new Vector3(0f, 0f, -0.5f) + new Vector3(0f, -0.5f, 0f));
		}
		points.Add(new Vector3(0f, -0.5f, -0.5f));
		points.Add(new Vector3(0f, 0.5f, -0.5f));
		points.Add(new Vector3(0f, -0.5f, 0.5f));
		points.Add(new Vector3(0f, 0.5f, 0.5f));
		points.Add(new Vector3(-0.5f, -0.5f, 0f));
		points.Add(new Vector3(-0.5f, 0.5f, 0f));
		points.Add(new Vector3(0.5f, -0.5f, 0f));
		points.Add(new Vector3(0.5f, 0.5f, 0));
		return points.ToArray();
	}

	private static Vector3[] CreateBoneWire() {
		float size = 1f/7f;
		List<Vector3> points = new List<Vector3>();
		points.Add(new Vector3(0.000f, 0.000f, 0.000f));
		points.Add(new Vector3(-size, -size, 0.200f));
		points.Add(new Vector3(0.000f, 0.000f, 0.000f));
		points.Add(new Vector3(size, -size, 0.200f));
		points.Add(new Vector3(0.000f, 0.000f, 0.000f));
		points.Add(new Vector3(-size, size, 0.200f));
		points.Add(new Vector3(0.000f, 0.000f, 0.000f));
		points.Add(new Vector3(size, size, 0.200f));

		points.Add(new Vector3(-size, -size, 0.200f));
		points.Add(new Vector3(0.000f, 0.000f, 1.000f));
		points.Add(new Vector3(size, -size, 0.200f));
		points.Add(new Vector3(0.000f, 0.000f, 1.000f));
		points.Add(new Vector3(-size, size, 0.200f));
		points.Add(new Vector3(0.000f, 0.000f, 1.000f));
		points.Add(new Vector3(size, size, 0.200f));
		points.Add(new Vector3(0.000f, 0.000f, 1.000f));

		points.Add(new Vector3(-size, -size, 0.200f));
		points.Add(new Vector3(size, -size, 0.200f));
		points.Add(new Vector3(size, -size, 0.200f));
		points.Add(new Vector3(size, size, 0.200f));
		points.Add(new Vector3(size, size, 0.200f));
		points.Add(new Vector3(-size, size, 0.200f));
		points.Add(new Vector3(-size, size, 0.200f));
		points.Add(new Vector3(-size, -size, 0.200f));
		return points.ToArray();
	}

}
