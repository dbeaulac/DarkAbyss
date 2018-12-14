/* Fonts used are 'Arcade' by Yuji Adachi and Bombardier by Apostrophic Labs
 * http://www.dafont.com/arcade-ya.font
 * http://www.dafont.com/bombardier.font
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{

	public static char[,] board = new char[8, 10];
	public static int[,] tread = new int[8, 10];
	Object[,] grid = new Object[8, 10];
	public static Object[,] sen = new Object[8, 10];
	Object[] q = new Object[5];
	char curr;
	int[] queue = new int[5];
	public static int startX, startY, finishX, finishY, startD, finishD, movement, cursorX, cursorY, freeze;
	public static int waves, wA, wK, wS, level, a, b, c, d;
	public static float moveV, moveH, threshold, delay, elapsed, limit, start;
	public static float interval, eff, pieces, score, swap, bonus, clear;
	public GameObject zero, one, two, three, hor, vert, cross, wave, sensor, wallV, wallH;
	Vector3 cP, tempD;
	public static Text LevelT, TimeT, ScoreT, ExtraT;
	public static bool active, fast, bon, splash, pause;
	string pauseHold;

	void Start ()
	{ 
		// Set up text objects
		LevelT = GameObject.Find ("LevelText").GetComponent<Text> ();
		TimeT = GameObject.Find ("TimeText").GetComponent<Text> ();
		ScoreT = GameObject.Find ("ScoreText").GetComponent<Text> ();
		ExtraT = GameObject.Find ("ExtraText").GetComponent<Text> ();

		// Sets up the internal array and midpoint sensors
		for (int j = 0; j < 10; j++) {
			for (int i = 0; i < 8; i++) {
				tempD = new Vector3 (j * 4.0f, 5.0f, i * -4.0f);
				// Adds the wave detection sensors
				sen [i, j] = Instantiate (sensor, tempD, Quaternion.identity);
			}
		}

		// Adds the vertical internal walls
		for (int i = 0; i < 11; i++) {
			tempD = new Vector3 ((i * 4.0f) - 2, 5.0f, -14.0f);
			Instantiate (wallV, tempD, Quaternion.identity);
		}

		// Adds the horizontal internal walls
		for (int i = 0; i < 9; i++) {
			tempD = new Vector3 (18.0f, 5.0f, 2 - (i * 4.0f));
			Instantiate (wallH, tempD, Quaternion.identity);
		}

		splash = true;
		active = false;
	}

	void InitLevel ()
	{
		// Remember that Random.Range returns between x to y-1
		startX = Random.Range (0, 10); // 0 is left column, 9 is right column
		startY = Random.Range (0, 8); // 0 is top row, 7 is bottom row

		// This block checks to see if the start point is bordering a wall, and turns it around
		if (startX == 0) {
			startD = 1; 
		} else if (startX == 9) {
			startD = 3;
		} else if (startY == 0) {
			startD = 2;
		} else if (startY == 7) {
			startD = 0;
		} else {
			startD = Random.Range (0, 4); // 0 is up, 1 is right, 2 is down, 3 is left
		}

		tempD = new Vector3 (0, startD * 90.0f, 0);
		GameObject.Find ("start").GetComponent<Rigidbody> ().transform.rotation = Quaternion.identity;
		GameObject.Find ("start").GetComponent<Rigidbody> ().transform.Rotate (tempD);

		// This block ensures that the finishing point is on the other side of the map
		if (startX < 3) {
			finishX = Random.Range (7, 10);
		} else if (startX > 6) {
			finishX = Random.Range (0, 3);
		} else if (startX == 3 || startX == 4) {
			finishX = Random.Range (8, 10);
		} else if (startX == 5 || startX == 6) {
			finishX = Random.Range (0, 2);
		} 

		finishY = Random.Range (0, 8); // 0 is top row, 7 is bottom row

		// This block checks to see if the finish point is bordering a wall, and turns it around
		if (finishX == 0) {
			finishD = 1; 
		} else if (finishX == 9) {
			finishD = 3;
		} else if (finishY == 0) {
			finishD = 2;
		} else if (finishY == 7) {
			finishD = 0;
		} else {
			finishD = Random.Range (0, 4); // 0 is up, 1 is right, 2 is down, 3 is left
		}

		tempD = new Vector3 (0, finishD * 90.0f, 0);
		GameObject.Find ("finish").GetComponent<Rigidbody> ().transform.rotation = Quaternion.identity;
		GameObject.Find ("finish").GetComponent<Rigidbody> ().transform.Rotate (tempD);

		// Sets up the internal array and midpoint sensors
		for (int j = 0; j < 10; j++) {
			for (int i = 0; i < 8; i++) {
				Destroy (grid [i, j]);
				board [i, j] = '-';
				tread [i, j] = 0;
			}
		}

		// Sets the start and finish on the internal array
		board [startY, startX] = 'S';
		board [finishY, finishX] = 'F';
		tread [startY, startX] = 1;
		tread [finishY, finishX] = 1;

		// Sets the start and finish points on the map
		Vector3 sP = new Vector3 (startX * 4, 5, startY * -4);
		Vector3 fP = new Vector3 (finishX * 4, 5, finishY * -4);
		GameObject.Find ("cursor").GetComponent<Rigidbody> ().position = sP;
		GameObject.Find ("start").GetComponent<Rigidbody> ().position = sP;
		GameObject.Find ("finish").GetComponent<Rigidbody> ().position = fP;
		cursorX = startX;
		cursorY = startY;
		cP = new Vector3 (cursorX, 5, cursorY);

		if (level == 1) {
			waves = 5; 					// number of waves at the beginning of the level
			start = 20.0f;				// time elapsed when a piece is released
			interval = 5.0f;			// gap between released pieces
			limit = 180.0f + start;		// amount of time to complete the level
			score = 0;					// score of the player
		} else {
			waves -= wK;				// deducts killed waves from total
			start = 20.0f - (level * 0.50f);
			limit = 185.0f - (level * 10.0f) + start;
		}

		movement = 0;		// prevents double movement of cursor
		freeze = 0;			// prevents double placement of pieces
		wA = 0;				// number of active waves
		wS = 0;				// number of saved waves
		wK = 0;				// number of killed waves
		delay = 0.0f;		// placement lockout upon replacing a piece
		elapsed = 0.0f;		// amount of time that has elapsed
		active = true;		// allows the player to play the game
		pause = false;		// pauses the game
		fast = false;		// allows the waves to move faster
		pieces = 0;			// number of pieces placed by the player
		a = 0;				// number of 'zero' pieces that have been dealt
		b = 0;				// number of 'one' pieces that have been dealt
		c = 0;				// number of 'two' pieces that have been dealt
		d = 0;				// number of 'three' pieces that have been dealt
		swap = 0;
		clear = 0;
		LevelT.text = level.ToString ();
		ExtraT.text = "";
		//GameObject.Find ("win").GetComponent<Rigidbody> ().position = new Vector3(-50.0f, 9.0f, -20.0f);
		//GameObject.Find ("lose").GetComponent<Rigidbody> ().position = new Vector3(-50.0f, 9.0f, -20.0f);

		// Sets up the initial queue
		for (int i = 0; i < 5; i++) {
			Destroy (q [i]);
			queue [i] = Ran ();
			QueuePiece (queue [i], -10.0f, -24.0f + (4.0f * i), i);
		}
	}

	void Update ()
	{
		int s;
		s = (int)Mathf.Floor (swap % 6.0f);

		if (active == true) {
			if (movement < 1) {
				// move cursor to the left
				if (Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.A)) {
					cursorX--;
					movement = 2;
				}

				// move cursor to the right
				if (Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.D)) {
					cursorX++;
					movement = 2;
				}

				// NOTE: on the map, the bottom row is at 'x = -28'
				// move cursor up
				if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.W)) {
					cursorY--;
					movement = 2;
				}

				// move cursor down
				if (Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.S)) {
					cursorY++;
					movement = 2;
				}

				// pauses the game
				if (Input.GetKeyDown (KeyCode.Return)) {
					active = false;
					pause = true;
					pauseHold = ExtraT.text;
					LevelT.text = "";
					ExtraT.text = "";
					TimeT.text = "";
					ScoreT.text = "";
					GameObject.Find ("pauseScreen").GetComponent<Rigidbody> ().position = new Vector3 (10.0f, 9.1f, -14.0f);
					GameObject.Find ("pauseScreen").GetComponent<Rigidbody> ().velocity = Vector3.zero;
				}
			} else {
				if (movement > 0) {
					movement--;	
				}
			}

			// clamps cursor inside the map
			if (cursorX < 0) {
				cursorX = 0;
			}
			if (cursorY < 0) {
				cursorY = 0;
			}
			if (cursorX > 9) {
				cursorX = 9;
			}
			if (cursorY > 7) {
				cursorY = 7;
			}

			// moves cursor to new location
			cP = new Vector3 (cursorX * 4, 5.0f, cursorY * -4);
			GameObject.Find ("cursor").GetComponent<Rigidbody> ().position = cP;
			//LevelT.text = "[" + cursorY + ", " + cursorX + "] = " + board [cursorY, cursorX] + "    " + waves + " [" + wA + " / " + wS + " / " + wK + "]    " + startD + " " + finishD;

			if (freeze < 1) {
				if (Input.GetKeyDown (KeyCode.Space)) {
					Place ();
					freeze = 2;
				}
			}

			if (!pause) {
				if (freeze > 0) {
					freeze--;
				}
				if (delay > 0) {
					delay -= Time.deltaTime;
					ExtraT.text = "QUEUE LOCK!";
					clear = delay;
				} 

				if (clear <= 0) {
					clear = 0;
					ExtraT.text = "";
				} 
			}

			elapsed += Time.deltaTime;
			limit -= Time.deltaTime;
			clear -= Time.deltaTime;

			if ((waves - wA - wK - wS) > 0 && elapsed > start) {
				Instantiate (wave, new Vector3 (startX * 4.0f, 8.0f, startY * -4.0f), Quaternion.identity);
				wA++;
				start += interval;
			}

			if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl)) {
				fast = true;
			} else {
				fast = false;
			}

			if (!pause) {
				TimeT.text = Mathf.Round (limit).ToString ();	
			}

			if ((wK + wS == waves && wA == 0) || limit < 0) {
				active = false;
				wK += wA;
				wA = 0;
				bon = true;
			}
		} else {
			if (splash == true) {
				TimeT.text = "";
				LevelT.text = "";
				ScoreT.text = "";
				ExtraT.text = "";
				GameObject.Find ("splash").GetComponent<Rigidbody> ().position = new Vector3 (18.0f, 9.0f, -14.0f);
				if (Input.GetKeyDown (KeyCode.Space)) {
					GameObject.Find ("splash").GetComponent<Rigidbody> ().position = new Vector3 (200.0f, 0.0f, 0.0f);
					level = 1;
					InitLevel ();
					splash = false;
				}
			} else {
				if (pause) {
					// unpauses the game
					if (Input.GetKeyDown (KeyCode.Return)) {
						active = true;
						pause = false;
						//TimeT.text;
						LevelT.text = level.ToString();
						ScoreT.text = ScoreT.text = Mathf.Round (score).ToString ();
						ExtraT.text = pauseHold;
						GameObject.Find ("pauseScreen").GetComponent<Rigidbody> ().position = new Vector3 (200.0f, 0.0f, 0.0f);
					}
					// quits the game
					if (Input.GetKeyDown (KeyCode.Space)) {
						GameObject.Find ("pauseScreen").GetComponent<Rigidbody> ().position = new Vector3 (200.0f, 0.0f, 0.0f);
						ExtraT.text = " GAME EXIT";	
						Application.Quit ();
					}
				} else {
					swap += Time.deltaTime;
					if (wS == 0) {
						if (s < 2) {
							ExtraT.text = " GAME OVER";	
						} else if (s > 3) {
							ExtraT.text = "TO RESTART";
						} else {
							ExtraT.text = "PRESS SPACE";
						}

						/*
					if (bon == true) {
						GameObject.Find ("lose").GetComponent<Rigidbody> ().position = new Vector3 (-10.0f, 8.0f, -20.0f);
						bon = false;
					}
					*/

						if (Input.GetKeyDown (KeyCode.Space)) {
							splash = true;
						}
					} else if (level == 10) {
						if (s < 2) {
							ExtraT.text = " YOU WIN!";	
						} else if (s > 3) {
							ExtraT.text = "TO RESTART";
						} else {
							ExtraT.text = "PRESS SPACE";
						}

						/*
					if (bon == true) {
						GameObject.Find ("win").GetComponent<Rigidbody> ().position = new Vector3(-10.0f, 8.0f, -20.0f);
					}
					*/

						AddBonus (bon);

						if (Input.GetKeyDown (KeyCode.Space)) {
							splash = true;
						}
					
					} else {
						if (s < 2) {
							if (bon == true) {
								ExtraT.text = " GOOD JOB!";
							} else {
								ExtraT.text = "TO CONTINUE";
							}
						} else if (s > 3) {
							ExtraT.text = "PRESS SPACE";
						} else {
							ExtraT.text = "BONUS:";
							AddBonus (bon);
							if (bonus > 9999) {
								ExtraT.text += bonus;
							}	
							ExtraT.text += " " + bonus;
						}

						if (Input.GetKeyDown (KeyCode.Space)) {
							level++;
							InitLevel ();
						}
					}
				}
			}
		}
	}

	void AddBonus (bool x)
	{
		if (x == true) {
			bonus = Mathf.Round (limit * ((100.0f * level) * (eff / pieces)));
			while (bonus % 100.0 != 0 && bonus > 1000) {
				bonus--;
			}
			UpScore (bonus);
			bon = false;
		}
	}

	int Ran ()
	{
		int r;
		bool valid = false;

		// Returns a random value from 0 to 6
		r = Random.Range (0, 70) % 7;

		while (valid == false) {
			if (r == 0 && a > 2) {
				r = Random.Range (0, 40) % 4;
			} else if (r == 1 && b > 2) {
				r = Random.Range (0, 40) % 4;
			} else if (r == 2 && c > 2) {
				r = Random.Range (0, 40) % 4;
			} else if (r == 3 && d > 2) {
				r = Random.Range (0, 40) % 4;
			} else {
				valid = true;
			}
		}

		switch (r) {
			case 0:
				a++;
				break;
			case 1:
				b++;
				break;
			case 2:
				c++;
				break;
			case 3:
				d++;
				break;
			default:
				break;
		}

		if (a + b + c + d == 12) {
			a = 0;
			b = 0;
			c = 0;
			d = 0;
		}

		return r;
	}

	char BoardPiece (int x)
	{
		switch (x) {
			case 0:
				return '0';
			case 1:
				return '1';
			case 2:
				return '2';
			case 3:
				return '3';
			case 4:
				return 'H';
			case 5:
				return 'V';
			case 6:
				return 'C';
			default:
				return '-';
		}
	}

	void QueuePiece (int n, float x, float z, int a)
	{
		switch (n) {
			case 0:
				q [a] = Instantiate (zero, new Vector3 (x, 5.0f, z), Quaternion.identity);
				break;
			case 1:
				q [a] = Instantiate (one, new Vector3 (x, 5.0f, z), Quaternion.identity);
				break;
			case 2:
				q [a] = Instantiate (two, new Vector3 (x, 5.0f, z), Quaternion.identity);
				break;
			case 3:
				q [a] = Instantiate (three, new Vector3 (x, 5.0f, z), Quaternion.identity);
				break;
			case 4:
				q [a] = Instantiate (hor, new Vector3 (x, 5.0f, z), Quaternion.identity);
				break;
			case 5:
				q [a] = Instantiate (vert, new Vector3 (x, 5.0f, z), Quaternion.identity);
				break;
			case 6:
				q [a] = Instantiate (cross, new Vector3 (x, 5.0f, z), Quaternion.identity);
				break;
			default:
				break;
		}
	}

	void PlacePiece (int n, float x, float z)
	{
		switch (n) {
			case 0:
				grid [cursorY, cursorX] = Instantiate (zero, new Vector3 (x * 4.0f, 5.0f, z * -4.0f), Quaternion.identity);
				break;
			case 1:
				grid [cursorY, cursorX] = Instantiate (one, new Vector3 (x * 4.0f, 5.0f, z * -4.0f), Quaternion.identity);
				break;
			case 2:
				grid [cursorY, cursorX] = Instantiate (two, new Vector3 (x * 4.0f, 5.0f, z * -4.0f), Quaternion.identity);
				break;
			case 3:
				grid [cursorY, cursorX] = Instantiate (three, new Vector3 (x * 4.0f, 5.0f, z * -4.0f), Quaternion.identity);
				break;
			case 4:
				grid [cursorY, cursorX] = Instantiate (hor, new Vector3 (x * 4.0f, 5.0f, z * -4.0f), Quaternion.identity);
				break;
			case 5:
				grid [cursorY, cursorX] = Instantiate (vert, new Vector3 (x * 4.0f, 5.0f, z * -4.0f), Quaternion.identity);
				break;
			case 6:
				grid [cursorY, cursorX] = Instantiate (cross, new Vector3 (x * 4.0f, 5.0f, z * -4.0f), Quaternion.identity);
				break;
			default:
				break;
		}
		pieces++;
	}

	void UpdateQueue ()
	{
		// Moves elements in the queue up one space
		for (int i = 1; i < 5; i++) {
			queue [i - 1] = queue [i];
		}

		// Generates new element for the free space in the queue
		queue [4] = Ran ();

		for (int i = 0; i < 5; i++) {
			Destroy (q [i]);
			QueuePiece (queue [i], -10.0f, -24.0f + (4.0f * i), i);
		}
	}

	void Place ()
	{
		if (tread [cursorY, cursorX] == 0 && delay <= 0.0f) {
			if (board [cursorY, cursorX] != '-') {
				Destroy (grid [cursorY, cursorX]);
				delay = 1.25f;
			}

			// Sets piece on board and tread arrays
			board [cursorY, cursorX] = BoardPiece (queue [0]);

			PlacePiece (queue [0], cursorX, cursorY);

			UpdateQueue ();
		} else {
			ExtraT.text = "PIECE LOCK!";
			clear = 2.0f;
		}
	}

	public static void UpScore (float x)
	{
		score += x;
		ScoreT.text = Mathf.Round (score).ToString ();
	}
}