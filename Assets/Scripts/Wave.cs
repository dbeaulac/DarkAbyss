using UnityEngine;
using System.Collections;

public class Wave : MonoBehaviour
{
	int dir, x, y, temp, p;
	//, f;
	float speed;

	void Start ()
	{
		dir = Level.startD;
		x = Level.startX;
		y = Level.startY;
		// f = Level.finishD;
		p = 0;
	}

	void Update ()
	{
		if (Level.active) {
			if (Level.fast == true) {
				speed = Time.deltaTime * 2;
			} else {
				speed = Time.deltaTime;
			}

			switch (dir) {
				case 0:
					this.GetComponent<Rigidbody> ().position += new Vector3 (0.0f, 0.0f, speed);
					break;
				case 1:
					this.GetComponent<Rigidbody> ().position += new Vector3 (speed, 0.0f, 0.0f);
					break;
				case 2:
					this.GetComponent<Rigidbody> ().position += new Vector3 (0.0f, 0.0f, -speed);
					break;
				case 3:
					this.GetComponent<Rigidbody> ().position += new Vector3 (-speed, 0.0f, 0.0f);
					break;
				default:
					break;
			}

			if (Level.limit < 0) {
				Destroy (gameObject);
				Level.wA--;
				Level.wK++;
			}
		}
	}

	// Checks to see if the next square is valid
	// First set of conditions checks to see if the wave is still in bounds
	// Second condition checks if next square has a piece
	// Third condition checks if the next square is valid
	// If all checks pass, then the next square is valid
	bool Check ()
	{
		if (x < 0) {
			Level.tread [y, 0] = 0;
			return false;
		} else if (y < 0) {
			Level.tread [0, x] = 0;
			return false;
		} else if (x > 9) {
			Level.tread [y, 9] = 0;
			return false;
		} else if (y > 7) {
			Level.tread [7, x] = 0;
			return false;
		} else if (Level.board [y, x] == '-') {
			return false;
		} else if (CheckD () == false) {
			return false;
			/*
		} else if (Level.board [y, x] == 'F') {
			if ((dir == 0 && f == 2) || (dir == 1 && f == 3) || (dir == 2 && f == 0) || (dir == 3 && f == 1)) {
				return true;
			} else {
				return false;
			}
			*/
		} else {
			Level.tread [y, x] = 1;
			return true;
		} 
	}

	bool CheckD ()
	{
		switch (dir) {
			case 0:
				if (Level.board [y, x] == '0' || Level.board [y, x] == '1' || Level.board [y, x] == 'H') {
					return false;
				} else {
					return true;
				}
			case 1:
				if (Level.board [y, x] == '0' || Level.board [y, x] == '3' || Level.board [y, x] == 'V') {
					return false;
				} else {
					return true;
				}
			case 2:
				if (Level.board [y, x] == '2' || Level.board [y, x] == '3' || Level.board [y, x] == 'H') {
					return false;
				} else {
					return true;
				}
			case 3:
				if (Level.board [y, x] == '1' || Level.board [y, x] == '2' || Level.board [y, x] == 'V') {
					return false;
				} else {
					return true;
				}
			default:
				return true;
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Wall") {
			switch (dir) {
				case 0:
					y--;
					break;
				case 1:
					x++;
					break;
				case 2:
					y++;
					break;
				case 3:
					x--;
					break;
				default:
					break;
			}
					
			if (Check () == false) {
				Destroy (gameObject);
				Level.wA--;
				Level.wK++;
				Level.ExtraT.text = "WAVE LOST!";
				Level.clear = 2.0f;
			}
		}

		if (other.tag == "Sensor" && Level.board [y, x] != 'S') {
			temp = dir;
			switch (Level.board [y, x]) {
				case '0':
					if (dir == 2) {
						dir = 1;
					} else if (dir == 3) {
						dir = 0;
					}

					break;
				case '1':
					if (dir == 2) {
						dir = 3;
					} else if (dir == 1) {
						dir = 0;
					}
					break;
				case '2':
					if (dir == 1) {
						dir = 2;
					} else if (dir == 0) {
						dir = 3;
					}
					break;
				case '3':
					if (dir == 3) {
						dir = 2;
					} else if (dir == 0) {
						dir = 1;
					}
					break;
				case 'F':
					Destroy (gameObject);
					Level.UpScore (Level.level * 900.0f);
					Level.wA--;
					Level.wS++;
					Level.eff = p;
					Level.ExtraT.text = "WAVE SAVED!";
					Level.clear = 2.0f;
					break;
				default:
					break;
			}

			if (temp != dir) {
				this.GetComponent<Rigidbody> ().position = new Vector3 (x * 4.0f, 8.0f, y * -4.0f);
			}
				
			Level.UpScore (Level.level * 100.0f);
			p++;
		}
	}
}
