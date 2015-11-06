using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class RoomGeneratorProd : MonoBehaviour {
	#region stored data
	public List<Rect> allRooms;
	public List<Rect> mainRooms;
	public List<Rect> halls = new List<Rect> ();
	public List<Rect> secondaryRoom = new List<Rect> ();
	public TileType[,] TileMapData;
	#endregion

	#region custom variables
	public float RoomRandomCircleRadius = 60.0f;
	public int RoomCount = 50;
	public int RoomMaxWidth = 30;
	public int RoomMinWidth = 9;
	public int RoomMaxHeight = 30;
	public int RoomMinHeight = 9;
	public int HallWidth = 3;
	public float MainRoomMulValue = 0.7f;
	public int HallOffsetOfEdge = 1;
	public float multiper = 1.1f;  // for seperation of rooms, speed up the time. Usually there's no need to modify this.
	#endregion

	#region private data
	private Dictionary<Rect, List<Rect>> neighborRoomCollection = new Dictionary<Rect, List<Rect>>();

	private static RoomGeneratorProd instance;
	private static GameObject gameobject;
	#endregion
	
	public enum TileType : byte
	{
		Empty = 0, // array default value is 0
		MainRoom,
		HallRoom,
		Hall
	}

	#region public functions

	// use this function to generate rooms
	public void GenerateRoom(){
		//Random.seed = 1;
		RandomAddRooms ();
		SeprateRooms ();
		MarkMainRooms ();
		FindNeighbors ();
		GenerateHallLines ();
		FindSecondaryRooms ();
	}

	static public RoomGeneratorProd GetInstance(){
		if (!instance) {
			gameobject = new GameObject ();
			gameobject.name = "RoomGeneratorProd";
			instance = gameobject.AddComponent(typeof(RoomGeneratorProd)) as RoomGeneratorProd;
		}
		return instance;
	}

	public void FillTilemap(){

		Vector2 offset = new Vector2 (int.MaxValue, int.MaxValue);
		int height = 0;
		int width = 0;

		// first, find the map size and offset.
		// allrooms does not contain halls, so we must foreach twice.
		foreach (Rect r in halls){
			if (r.xMin < offset.x) offset.x = 	(int) r.xMin;
			if (r.yMin < offset.y) offset.y = 	(int) r.yMin;
			if (r.yMax > height) height =   	(int) r.yMax;
			if (r.xMax > width) width =      	(int) r.xMax;
		}
		foreach (Rect r in allRooms){
			if (r.xMin < offset.x) offset.x = 	(int) r.xMin;
			if (r.yMin < offset.y) offset.y = 	(int) r.yMin;
			if (r.yMax > height) height = 	(int) r.yMax;
			if (r.xMax > width) width = 		(int) r.xMax;
		}
		TileMapData = new TileType[width - (int)offset.x, height - (int)offset.y];

		MarkTileWithRect (halls, TileType.Hall, offset);
		MarkTileWithRect (secondaryRoom, TileType.HallRoom, offset);
		MarkTileWithRect (mainRooms, TileType.MainRoom, offset);

	}


	#endregion

	#region private functions

	void MarkTileWithRect(List<Rect> list, TileType t, Vector2 offset){
		foreach (Rect r in list) {
			for (int x = (int)r.xMin; x < (int)r.xMax; x++) {
				for (int y = (int)r.yMin; y < (int)r.yMax; y++) {
					TileMapData [x - (int)offset.x, y - (int)offset.y] = t; 
				}
			}
		}
	}
	
	Vector2 GetRandomPointInCircle(float radius){
		float t = 2 * Mathf.PI * Random.Range (0.0f, 1.0f);
		float u = Random.Range (0.0f, 1.0f) + Random.Range (0.0f, 1.0f);
		float r;
		if (u > 1)
			r = 2 - u;
		else
			r = u;
		return new Vector2(radius * r * Mathf.Cos(t), radius * r * Mathf.Sin(t));
	}


	void RandomAddRooms(){
		allRooms = new List<Rect> ();
		for (int i = 0; i < RoomCount; i ++) {
			Vector2 r = GetRandomPointInCircle (RoomRandomCircleRadius);
			Rect room = new Rect (Mathf.Round (r.x + RoomRandomCircleRadius),
			                     Mathf.Round (r.y + RoomRandomCircleRadius),
			                     Mathf.Round (Random.Range (RoomMinWidth, RoomMaxWidth)),
			                     Mathf.Round (Random.Range (RoomMinHeight, RoomMaxHeight))
			);
			allRooms.Add (room);
		}
	}

	void SeprateRooms(){
		int roomCount = allRooms.Count;
		bool touching;
		do {
			touching = false;
			
			for (int i = 0; i < roomCount; i ++) {
				Rect a = allRooms[i];
				for (int j = i + 1; j < roomCount; j++) {
					Rect b = allRooms[j];
					if (a.Overlaps(b)){
						touching = true;
						float dx = Mathf.Min ( a.xMax - b.xMin, a.x - (b.xMax) );
						float dy = Mathf.Min ( a.y - (b.yMax), (a.yMax) - b.y);
						
						if (Mathf.Abs(dx) < Mathf.Abs(dy)){
							float dxa = -dx / 2;
							float dxb = dx + dxa;
							a.x += dxa * multiper;
							b.x += dxb * multiper;
						}
						else {
							float dya = -dy/2;
							float dyb = dy + dya;
							b.y += dyb * multiper;
							a.y += dya * multiper;
						}
						a.x = Mathf.Floor(a.x);
						a.y = Mathf.Floor(a.y);
						b.y = Mathf.Floor(b.y);
						b.x = Mathf.Floor(b.x);

						allRooms[i] = a;
						allRooms[j] = b;
					}

				}
			}
		} while (touching == true);
	}

	float GetDist(Rect a, Rect b){
		return Mathf.Pow(a.center.x - b.center.x, 2) + Mathf.Pow(a.center.y - b.center.y , 2);
	}
	
	void MarkMainRooms(){
		mainRooms = new List<Rect>();
		float minWidth = RoomMaxWidth * MainRoomMulValue;
		float minHeight = RoomMaxHeight * MainRoomMulValue;
		foreach (Rect r in allRooms) {
			if (r.height > minHeight && r.width > minWidth){
				mainRooms.Add(r);
			}
		}
	}

	void FindNeighbors(){
		Rect a, b, c;
		float abDist, acDist, bcDist;
		bool skip;
		int roomCount = mainRooms.Count;
		for (int i = 0; i < roomCount; i ++) {
			a = mainRooms[i];
			for (int j = i + 1; j < roomCount; j++){
				skip = false;
				b = mainRooms[j];
				abDist = GetDist(a, b);
				for (int k =0; k < roomCount; k ++){
					if(k ==i || k == j) continue;
					c = mainRooms[k];
					acDist = GetDist(a, c);
					bcDist = GetDist(b, c);
					if(acDist < abDist && bcDist < abDist)
						skip = true;
					if(skip)
						break;
				}
				if(!skip){
					if(! neighborRoomCollection.ContainsKey(a)){
						neighborRoomCollection.Add (a, new List<Rect>());
					}
					neighborRoomCollection[a].Add(b);
				}
			}
		}
	}

	void GenerateHallLines(){
		foreach (Rect a in neighborRoomCollection.Keys) {
			foreach(Rect b in neighborRoomCollection[a]){
				
				Vector2 p1, p2 ,p3 ,p4;
				p1 = new Vector2(a.x,  a.y +    a.height);
				p2 = new Vector2(a.x + a.width, a.y);
				p3 = new Vector2(b.x,  b.y +    b.height);
				p4 = new Vector2(b.x + b.width, b.y);
				
				float minXdiff = 2 * HallOffsetOfEdge + HallWidth;
				if (p2.x - p3.x > minXdiff && p4.x - p1.x > minXdiff ){
					Rect Hall = new Rect(
						p4.x > p2.x ? p2.x - HallOffsetOfEdge - HallWidth : p4.x - HallOffsetOfEdge - HallWidth ,
						p3.y <= p2.y ? p3.y : p1.y,
						HallWidth,
						p3.y <= p2.y ? Mathf.Abs(p2.y - p3.y) : Mathf.Abs(p4.y - p1.y)
						);
					halls.Add(Hall);
					continue;
				}
				
				float minYdiff = 2 * HallOffsetOfEdge + HallWidth;
				if (p3.y - p2.y > minYdiff && p1.y - p4.y > minYdiff){
					Rect Hall = new Rect(
						p3.x >= p2.x ? p2.x : p4.x,
						p1.y < p3.y ? p1.y - HallOffsetOfEdge - HallWidth : p3.y - HallOffsetOfEdge - HallWidth,
						p2.x <= p3.x ? Mathf.Abs(p3.x - p2.x) : Mathf.Abs(p1.x - p4.x),
						HallWidth
						);
					halls.Add(Hall);
					continue;
				}
				
				Rect ra, rb;
				if (a.center.x < b.center.x){
					ra = a;
					rb = b;
				}else{
					ra = b;
					rb = a;
				}
				
				float x =  Mathf.Floor(ra.center.x);
				float y =  Mathf.Floor(ra.center.y);
				float dx = Mathf.Floor(rb.center.x - x);
				float dy = Mathf.Floor(rb.center.y - y);
				if (Random.value > 0.5f){
					Rect Hall = new Rect(x,	y, dx + HallWidth, HallWidth);
					halls.Add(Hall);
					if(dy > 0){
						Hall = new Rect(x + dx , y, HallWidth, dy);
						halls.Add(Hall);
					}else{
						Hall = new Rect(x + dx , y + dy, HallWidth, -dy);
						halls.Add(Hall);
					}

				}else{
					Rect Hall = new Rect(x,	y + dy, dx + HallWidth, HallWidth);
					halls.Add(Hall);

					if(dy > 0){
						Hall = new Rect(x, y, HallWidth, dy);
						halls.Add(Hall);
					}else{
						Hall = new Rect(x, y + dy, HallWidth, -dy);
						halls.Add(Hall);
					}
				}
			}
		}
	}
	
	void FindSecondaryRooms(){
		secondaryRoom = new List<Rect> ();
		foreach (Rect r in allRooms) {
			foreach(Rect h in halls){
				if (!(mainRooms.Contains(r)) &&  r.Overlaps(h))
					secondaryRoom.Add(r);
			}
		}
	}
	
	#endregion
}

