// DebugX.cs
// Hayden Scott-Baron (Dock) - http://starfruitgames.com
// Adds a number of useful Debug Draw features

using UnityEngine;
using System.Collections;

public class DebugX : MonoBehaviour
{
	public static void DrawCube(Vector3 pos, Color col, Vector3 scale)
	{
		Vector3 halfScale = scale * 0.5f;
		
		Vector3[] points = new Vector3[]
		{
			pos + new Vector3(halfScale.x,      halfScale.y,    halfScale.z),
			pos + new Vector3(-halfScale.x,     halfScale.y,    halfScale.z),
			pos + new Vector3(-halfScale.x,     -halfScale.y,   halfScale.z),
			pos + new Vector3(halfScale.x,      -halfScale.y,   halfScale.z),
			pos + new Vector3(halfScale.x,      halfScale.y,    -halfScale.z),
			pos + new Vector3(-halfScale.x,     halfScale.y,    -halfScale.z),
			pos + new Vector3(-halfScale.x,     -halfScale.y,   -halfScale.z),
			pos + new Vector3(halfScale.x,      -halfScale.y,   -halfScale.z),
		};
		
		Debug.DrawLine(points[0], points[1], col);
		Debug.DrawLine(points[1], points[2], col);
		Debug.DrawLine(points[2], points[3], col);
		Debug.DrawLine(points[3], points[0], col);
	}
	
	public static void DrawRect(Rect rect, Color col)
	{
		Vector3 pos = new Vector3(rect.x + rect.width / 2, rect.y + rect.height / 2, 0.0f);
		Vector3 scale = new Vector3(rect.width, rect.height, 0.0f);
		
		DebugX.DrawRect(pos, col, scale);
	}
	
	public static void DrawRect(Rect rect, Color col, float scale)
	{
		Vector3 pos = new Vector3((rect.x + rect.width / 2) * scale, (rect.y + rect.height / 2) * scale, 0.0f);
		Vector3 dimensions = new Vector3(rect.width * scale, rect.height * scale, 0.0f);
		
		DebugX.DrawRect(pos, col, dimensions);
	}
	
	public static void DrawRect(Vector3 pos, Color col, Vector3 scale)
	{
		Vector3 halfScale = scale * 0.5f;
		
		Vector3[] points = new Vector3[]
		{
			pos + new Vector3(halfScale.x,      halfScale.y,    halfScale.z),
			pos + new Vector3(-halfScale.x,     halfScale.y,    halfScale.z),
			pos + new Vector3(-halfScale.x,     -halfScale.y,   halfScale.z),
			pos + new Vector3(halfScale.x,      -halfScale.y,   halfScale.z),
		};
		
		Debug.DrawLine(points[0], points[1], col);
		Debug.DrawLine(points[1], points[2], col);
		Debug.DrawLine(points[2], points[3], col);
		Debug.DrawLine(points[3], points[0], col);
	}
	
	public static void DrawPoint(Vector3 pos, Color col, float scale)
	{
		Vector3[] points = new Vector3[]
		{
			pos + (Vector3.up * scale),
			pos - (Vector3.up * scale),
			pos + (Vector3.right * scale),
			pos - (Vector3.right * scale),
			pos + (Vector3.forward * scale),
			pos - (Vector3.forward * scale)
		};
		
		Debug.DrawLine(points[0], points[1], col);
		Debug.DrawLine(points[2], points[3], col);
		Debug.DrawLine(points[4], points[5], col);
		
		Debug.DrawLine(points[0], points[2], col);
		Debug.DrawLine(points[0], points[3], col);
		Debug.DrawLine(points[0], points[4], col);
		Debug.DrawLine(points[0], points[5], col);
		
		Debug.DrawLine(points[1], points[2], col);
		Debug.DrawLine(points[1], points[3], col);
		Debug.DrawLine(points[1], points[4], col);
		Debug.DrawLine(points[1], points[5], col);
		
		Debug.DrawLine(points[4], points[2], col);
		Debug.DrawLine(points[4], points[3], col);
		Debug.DrawLine(points[5], points[2], col);
		Debug.DrawLine(points[5], points[3], col);
		
	}
}