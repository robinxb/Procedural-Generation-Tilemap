using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class RoomGeneratorControllor : MonoBehaviour {
	public enum TileType : byte
	{
		Empty = 0, // array default value is 0
		MainRoom,
		HallRoom,
		Hall
	}

	private List<Room> rooms = new List<Room>();
	private List<Room> mainRooms = new List<Room>();
	private class LineSegment{
		public Vector2 start;
		public Vector2 stop;

		public LineSegment(Vector2 start, Vector2 stop){
			this.start = start;
			this.stop = stop;
		}
	}

	private Dictionary<Room, List<LineSegment>> neighborGraph = new Dictionary<Room, List<LineSegment>> ();
	private Dictionary<Room, List<Room>> neighborRoomCollection = new Dictionary<Room, List<Room>>();
	private List<Room> halls = new List<Room> ();

	public float RoomRandomCircleRadius = 200.0f;
	public int RoomCount = 100;
	public int RoomMaxWidth = 30;
	public int RoomMinWidth = 3;
	public int RoomMaxHeight = 30;
	public int RoomMinHeight = 3;
	public int HallWidth = 3;
	public float MainRoomMulValue = 0.7f;
	public int HallOffsetOfEdge = 1;
	public Camera cam;
	public Room roomObj;

	private TileType[,] TileMapData;

	void Start () {
		Time.timeScale = 10;
		StartCoroutine (GenStep1 ());
		GenStep1 ();
	}

	IEnumerator GenStep1(){
		rooms.Clear ();
		for (int i = 0; i < RoomCount; i ++) {
			Vector2 r = GetRandomPointInCircle(RoomRandomCircleRadius);
			Room room = Instantiate(roomObj) as Room;
			room.pos = new Rect(Mathf.Round(r.x + RoomRandomCircleRadius),
			                    Mathf.Round(r.y + RoomRandomCircleRadius),
			            		Mathf.Round(Random.Range(RoomMinWidth, RoomMaxWidth)),
			            		Mathf.Round(Random.Range(RoomMinHeight, RoomMaxHeight))
			                    );
			rooms.Add(room);
			yield return new WaitForSeconds (0.05f);
		}
		StartCoroutine (SeprateRooms ());
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

	bool TouchTest(Room a, Room b){
		return a.pos.Overlaps (b.pos);
	}

	IEnumerator SeprateRooms(){
		int roomCount = rooms.Count;
		bool touching;
		do {
			touching = false;
			for (int i = 0; i < roomCount; i ++) {
				Room a = rooms[i];
				for (int j = i + 1; j < roomCount; j++) {
					Room b = rooms[j];
					a.pos.x = Mathf.Floor(a.pos.x);
					a.pos.y = Mathf.Floor(a.pos.y);
					b.pos.y = Mathf.Floor(b.pos.y);
					b.pos.x = Mathf.Floor(b.pos.x);
					if (TouchTest(a, b)){
						touching = true;
						float dx = Mathf.Min (a.GetRight() - b.GetLeft(), a.GetLeft() - b.GetRight());
						float dy = Mathf.Min (a.GetBottom() - b.GetTop(), a.GetTop() - b.GetBottom());

						if (Mathf.Abs(dx) < Mathf.Abs(dy)){
							float dxa = -dx / 2;
							float dxb = dx + dxa;
							a.pos.x += dxa;
							b.pos.x += dxb;
						}
						else {
							float dya = -dy/2;
							float dyb = dy + dya;
							b.pos.y += dyb;
							a.pos.y += dya;
						}
					}
				}
				yield return null;
			}
		} while (touching == true);
		StartCoroutine(MarkMainRooms());
	}

	IEnumerator MarkMainRooms(){
		mainRooms.Clear ();
		float minWidth = RoomMaxWidth * MainRoomMulValue;
		float minHeight = RoomMaxHeight * MainRoomMulValue;
		foreach (Room r in rooms) {
			if (r.pos.height > minHeight && r.pos.width > minWidth){
				r.mainRoom = true;
				mainRooms.Add(r);
			}
			yield return new WaitForSeconds (0.5f);
		}
		StartCoroutine (FindNeighbors ());
	}

	float GetDist(Room a, Room b){
		return Mathf.Pow(a.pos.center.x - b.pos.center.x, 2) + Mathf.Pow(a.pos.center.y - b.pos.center.y , 2);
	}

	IEnumerator FindNeighbors(){
		Room a, b, c;
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
					acDist = GetDist(a,c);
					bcDist = GetDist(b,c);
					if(acDist < abDist && bcDist < abDist)
						skip = true;
					if(skip)
						break;
				}
				if(!skip){
					if(! neighborGraph.ContainsKey(a)){
						neighborGraph.Add(a, new List<LineSegment>());
						neighborRoomCollection.Add (a, new List<Room>());
					}
					neighborGraph[a].Add(new LineSegment(a.pos.center, b.pos.center));
					neighborRoomCollection[a].Add(b);
					yield return new WaitForSeconds(0.001f);
				}
			}
		}
		StartCoroutine (GenerateHallLines ());
	}

	IEnumerator GenerateHallLines(){
		foreach (Room a in neighborRoomCollection.Keys) {
			foreach(Room b in neighborRoomCollection[a]){

				Vector2 p1, p2 ,p3 ,p4;
				p1 = new Vector2(a.pos.x, a.pos.y + a.pos.height);
				p2 = new Vector2(a.pos.x + a.pos.width, a.pos.y);
				p3 = new Vector2(b.pos.x, b.pos.y + b.pos.height);
				p4 = new Vector2(b.pos.x + b.pos.width, b.pos.y);

				float minXdiff = 2 * HallOffsetOfEdge + HallWidth;
				if (p2.x - p3.x > minXdiff && p4.x - p1.x > minXdiff ){
					Rect Hall = new Rect(
						p4.x > p2.x ? p2.x - HallOffsetOfEdge - HallWidth : p4.x - HallOffsetOfEdge - HallWidth ,
						p3.y <= p2.y ? p3.y : p1.y,
						HallWidth,
						p3.y <= p2.y ? Mathf.Abs(p2.y - p3.y) : Mathf.Abs(p4.y - p1.y)
					);
					Room r = Instantiate(roomObj) as Room;
					r.pos = Hall;
					r.isHall = true;
					halls.Add(r);
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
					Room r = Instantiate(roomObj) as Room;
					r.pos = Hall;
					r.isHall = true;
					halls.Add(r);
					continue;
				}

				Room ra, rb;
				if (a.pos.center.x < b.pos.center.x){
					ra = a;
					rb = b;
				}else{
					ra = b;
					rb = a;
				}

				float x = ra.pos.center.x;
				float y = ra.pos.center.y;
				float dx = rb.pos.center.x - x;
				float dy = rb.pos.center.y - y;

				if (Random.value > 0.5f){
					Rect Hall = new Rect(x,	y, dx + 1, 1);
					Room r = Instantiate(roomObj) as Room;
					r.pos = Hall;
					r.isHall = true;
					halls.Add(r);

					Hall = new Rect(x + dx , y, 1, dy);
					r = Instantiate(roomObj) as Room;
					r.pos = Hall;
					r.isHall = true;
					halls.Add(r);
				}else{
					Rect Hall = new Rect(x,	y + dy, dx + 1, 1);
					Room r = Instantiate(roomObj) as Room;
					r.pos = Hall;
					r.isHall = true;
					halls.Add(r);
					
					Hall = new Rect(x, y, 1, dy);
					r = Instantiate(roomObj) as Room;
					r.pos = Hall;
					r.isHall = true;
					halls.Add(r);
				}
				yield return null;
			}
		}
		StartCoroutine (MarkHalls ());
	}

	IEnumerator MarkHalls(){
		foreach (Room h in halls) {
			h.pos.x = Mathf.Floor(h.pos.x) - 1;
			h.pos.y = Mathf.Floor(h.pos.y) - 1;
			h.pos.height = Mathf.Floor(h.pos.height) + 2;
			h.pos.width = Mathf.Floor(h.pos.width) + 2;
		}

		foreach (Room r in rooms){
			if (r.mainRoom) continue;

			foreach (Room h in halls) {
				if (!r.mainRoom && TouchTest(h, r)){
					Debug.Log("aaa");
					r.isHall = true;
				}
			}
			if (!r.mainRoom && (!r.isHall)){
				r.disable = true;
			}
			yield return null;
		}

		StartCoroutine (GenerateTileMap ());
	}

	IEnumerator GenerateTileMap(){
		int MapStartX = (int) (rooms [0].pos.x);
		int MapStartY = (int) (rooms [0].pos.y);
		int MapHeight = (int) (rooms [0].pos.height);
		int MapWidth =  (int) (rooms [0].pos.height);
		foreach (Room r in rooms) {
			if (r.disable) continue;
			if (r.pos.x < MapStartX) MapStartX = (int) (r.pos.x);
			if (r.pos.y < MapStartY) MapStartY = (int) (r.pos.y);
			if (r.pos.x + r.pos.width > MapWidth) MapWidth =  (int)(r.pos.x + r.pos.width);
			if (r.pos.y + r.pos.height > MapHeight) MapHeight = (int) (r.pos.y + r.pos.height);
		}
		foreach (Room r in halls) {
			if (r.pos.x < MapStartX) MapStartX = (int) (r.pos.x);
			if (r.pos.y < MapStartY) MapStartY = (int) (r.pos.y);
			if (r.pos.x + r.pos.width > MapWidth) MapWidth =    (int) (r.pos.x + r.pos.width);
			if (r.pos.y + r.pos.height > MapHeight) MapHeight = (int) (r.pos.y + r.pos.height);         
		}

		// first for main rooms
		TileMapData = new TileType[MapHeight, MapWidth];
		foreach (Room r in mainRooms) {
			for (int x = (int) (r.pos.x - MapStartX); x < (int) (r.pos.x - MapStartX + r.pos.width); x++ ){
				for (int y = (int) (r.pos.y - MapStartY); y < (int) (r.pos.y - MapStartY + r.pos.height); y++ ){
					if (TileMapData[x, y] != TileType.Empty) continue;
					TileMapData[x, y] = TileType.MainRoom;
				}
			}
		}

		foreach (Room r in rooms) {
			if (r.mainRoom || r.disable) continue;
			for (int x = (int) (r.pos.x - MapStartX); x < (int) (r.pos.x - MapStartX + r.pos.width); x++ ){
				for (int y = (int) (r.pos.y - MapStartY); y < (int) (r.pos.y - MapStartY + r.pos.height); y++ ){
					if (TileMapData[x, y] != TileType.Empty) continue;
					TileMapData[x, y] = TileType.HallRoom;
				}
			}
		}

		foreach (Room r in halls) {
			if (r.mainRoom || r.disable) continue;
			for (int x = (int) (r.pos.x - MapStartX); x < (int) (r.pos.x - MapStartX + r.pos.width); x++ ){
				for (int y = (int) (r.pos.y - MapStartY); y < (int) (r.pos.y - MapStartY + r.pos.height); y++ ){
					if (TileMapData[x, y] != TileType.Empty) continue;
					TileMapData[x, y] = TileType.Hall;
				}
			}
		}

		// clean
		foreach (Room r in halls) {
			Destroy(r);
		}
		halls.Clear ();

		foreach (Room r in rooms) {
			if (!r.mainRoom)
				Destroy(r);
		}
		rooms.Clear ();

		yield return null;
	}

	void Update(){
		foreach (Room r in neighborGraph.Keys) {
			foreach(LineSegment l in neighborGraph[r]){
				Debug.DrawLine(l.start, l.stop, new Color(1f, 1f,0f,0.5f));
			}
		}
	}
}
