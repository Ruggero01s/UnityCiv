using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GridController : MonoBehaviour
{
	public List<Hexagon> waterHexagons = new List<Hexagon>();
	public List<Hexagon> mountainHexagons = new List<Hexagon>();
	public List<Hexagon> plainHexagons = new List<Hexagon>();
	public Vector2Int gridSize;
	public GameObject plainHex;
	public GameObject fertilePlainHex;
	public GameObject waterHex;
	public GameObject mountainHex;
	public GameObject forestHex;
	public GameObject oceanHex;

	public GameObject ownerOverlayLoad;
	public static GameObject ownerOverlay;

	public GameObject playerCityModel;
	public GameObject enemyCityModel;
	public GameObject playerUnitModel;
	public GameObject enemyUnitModel;
	public List<HexagonGame> possibleSpawnHexes = new List<HexagonGame>();
	public int STARTING_UNITS = 2;
	public int STARTING_UNIT_ATK = 6;
	public int STARTING_UNIT_DEF = 4;
	public int STARTING_UNIT_MAXHP = 10;

	public Hexagon[,] hexagons;
	public HexagonGame[,] gameHexagons;

	// Noise parameters
	public float noiseSeedAlt;
	public float noiseSeedForest;


	public Vector3 scale = new Vector3(9.98f, 10, 8.66f);
	public float hexSizeX = 9.98f;
	public float hexSizeY = 8.66f;

	public Unit selectedUnit;
	public HexagonGame selectedTerrain;

	public Pathfinding pathfinder;

	public List<Unit> playerUnits = new List<Unit>();
	public List<Unit> enemyUnits = new List<Unit>();
	public Player player;

	public Player enemy;
	public City playerCity;

	public City enemyCity;

	public TurnManager turnManager;
	public GeneralHUDController HUDctrl;

	private bool firstUpdate = false;

	// Start is called before the first frame update
	void Start()
	{
		ownerOverlay = ownerOverlayLoad;
		hexagons = new Hexagon[gridSize.x, gridSize.y];
		gameHexagons = new HexagonGame[gridSize.x, gridSize.y];
		noiseSeedAlt = Random.Range(0.1f, 0.5f);
		noiseSeedForest = Random.Range(0.1f, 0.5f);
		GenerateHexMap(gridSize);

		foreach (HexagonGame gameHex in gameHexagons)
		{
			if (gameHex.CompareTag("MovableTerrain"))
			{
				possibleSpawnHexes.Add(gameHex);
			}
		}

		InitializePlayers();

		SpawnCitiesAndUnits();

		// Find the TurnManager in the scene
		turnManager = FindObjectOfType<TurnManager>();
		HUDctrl = FindObjectOfType<GeneralHUDController>();

		firstUpdate = true;
	}


	// Update is called once per frame
	void Update()
	{
		if (firstUpdate)
		{
			List<HexagonGame> neighbors = GetGameNeighbors(playerCity.gameHex);
			for (int i = 0; i < neighbors.Count; i++)
			{
				if (neighbors[i].hexType.Equals(waterHex))
				{
					neighbors.RemoveAt(i);
					i--;
				}
				else
				{
					neighbors[i].ClaimHex(player);
				}
			}

			neighbors = GetGameNeighbors(enemyCity.gameHex);
			for (int i = 0; i < neighbors.Count; i++)
			{
				if (neighbors[i].hexType.Equals(waterHex))
				{
					neighbors.RemoveAt(i);
					i--;
				}
				else
				{
					neighbors[i].ClaimHex(enemy);
				}
			}
			firstUpdate = false;
		}
	}

	public void GenerateHexMap(Vector2Int gridSize)
	{
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				Hexagon hex = new Hexagon(new Vector2Int(x, y));
				hex.rawPosition = GetPosForHexFromCoord(hex.coordinates);
				hex.hexType = DetermineTileMainType(hex.coordinates);
				if (hex.hexType.Equals(waterHex))
					waterHexagons.Add(hex);
				else if (hex.hexType.Equals(plainHex))
					plainHexagons.Add(hex);
				else if (hex.hexType.Equals(mountainHex))
					mountainHexagons.Add(hex);
				//Debug.Log("x: " + x + "  y: " + y);
				hexagons[x, y] = hex;
			}
		}

		ForestGen();

		hexagons = SmoothGen(hexagons);
		HexRenderer(hexagons);
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				Hexagon hex = hexagons[x, y];
				HexagonGame gameHex = gameHexagons[x, y];
				gameHex.hexType = hex.hexType;
			}
		}
		pathfinder = new Pathfinding(this, gameHexagons, gridSize.x, gridSize.y);
	}

	public Vector3 GetPosForHexFromCoord(Vector2Int coordinates)
	{
		float xOffset = hexSizeX; // 3/4 width offset for staggered columns
		float yOffset = hexSizeY * 0.866f;

		// Calculate the position with staggered columns
		float posX = coordinates.x * xOffset;
		float posY = coordinates.y * yOffset;

		// Offset every other row
		if (coordinates.y % 2 == 1)
		{
			posX += hexSizeX * 0.5f;
		}

		return new Vector3(posX, 0, -posY);

	}

	public GameObject DetermineTileMainType(Vector2Int coords)
	{
		// Generate a Perlin noise value
		float xCoord = coords.x * noiseSeedAlt;
		float yCoord = coords.y * noiseSeedAlt;
		float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);
		// Debug.Log(noiseValue);
		// Determine tile type based on noise value
		if (noiseValue < 0.25f)
		{
			return waterHex;
		}
		else if (noiseValue < 0.75f)
		{
			return plainHex;
		}
		else
		{
			return mountainHex;
		}
	}

	public Hexagon[,] SmoothGen(Hexagon[,] hexes)
	{
		int[,] countWaterHexs = new int[gridSize.x, gridSize.y];
		int[,] countPlainHexs = new int[gridSize.x, gridSize.y];
		int[,] countMountainHexs = new int[gridSize.x, gridSize.y];


		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				foreach (Hexagon neighbour in GetNeighbors(hexes[x, y]))
				{
					if (neighbour != null)
						if (neighbour.hexType.Equals(waterHex))
							countWaterHexs[x, y]++;
				}
				if (hexes[x, y].hexType.Equals(waterHex) && countWaterHexs[x, y] <= 0)
					hexes[x, y].hexType = plainHex;
			}
		}

		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				foreach (Hexagon neighbour in GetNeighbors(hexes[x, y]))
				{
					if (neighbour != null)
						if (neighbour.hexType.Equals(waterHex))
							countWaterHexs[x, y]++;
				}
				if (hexes[x, y].hexType.Equals(plainHex) && countWaterHexs[x, y] >= 2)
					hexes[x, y].hexType = fertilePlainHex;
			}
		}
		return hexes;
	}


	public void HexRenderer(Hexagon[,] hexes)
	{
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				GameObject newHex = Instantiate(hexes[x, y].hexType, hexes[x, y].rawPosition, Quaternion.identity);
				newHex.transform.localScale = scale;
				newHex.transform.parent = gameObject.transform;
				newHex.AddComponent<HexagonGame>();
				newHex.GetComponent<HexagonGame>().coordinates = new Vector2Int(x, y);
				newHex.GetComponent<HexagonGame>().rawPosition = newHex.transform.position;

				if (!hexes[x, y].hexType.Equals(waterHex))
					newHex.tag = "MovableTerrain";
				gameHexagons[x, y] = newHex.GetComponent<HexagonGame>();
			}
		}
	}

	public List<Hexagon> GetNeighbors(Hexagon hex)
	{
		int x = hex.coordinates.x;
		int y = hex.coordinates.y;
		if (y % 2 == 1)
		{
			return new List<Hexagon>
			{
				(y-1>=0)                            ? hexagons[x,y - 1] : null,
				(x+1<gridSize.x && y-1>=0)          ? hexagons[x+1,y-1] : null,
				(x+1<gridSize.x)                    ? hexagons[x+1,y] : null,
				(x+1<gridSize.x && y+1<gridSize.y)  ? hexagons[x+1,y+1] : null,
				(y+1<gridSize.y)                    ? hexagons[x,y+1] : null,
				(x-1>=0)                            ? hexagons[x-1,y] : null
			};
		}
		else
		{
			return new List<Hexagon>
			{
				(x-1>=0 && y-1>=0)                  ? hexagons[x-1,y - 1] : null,
				(y-1>=0)                            ? hexagons[x,y-1] : null,
				(x+1<gridSize.x)                    ? hexagons[x+1,y] : null,
				(y+1<gridSize.y)                    ? hexagons[x,y+1] : null,
				(x-1>=0 && y+1<gridSize.y)          ? hexagons[x-1,y+1] : null,
				(x-1>=0)                            ? hexagons[x-1,y] : null
			};
		}
	}

	public List<HexagonGame> GetGameNeighbors(HexagonGame hex)
	{
		int x = hex.coordinates.x;
		int y = hex.coordinates.y;

		var neighbors = new List<HexagonGame>();

		if (y % 2 == 1)
		{
			neighbors = new List<HexagonGame>
		{
			(y-1 >= 0)                           ? gameHexagons[x, y - 1] : null,
			(x+1 < gridSize.x && y-1 >= 0)       ? gameHexagons[x+1, y-1] : null,
			(x+1 < gridSize.x)                   ? gameHexagons[x+1, y] : null,
			(x+1 < gridSize.x && y+1 < gridSize.y)? gameHexagons[x+1, y+1] : null,
			(y+1 < gridSize.y)                   ? gameHexagons[x, y+1] : null,
			(x-1 >= 0)                           ? gameHexagons[x-1, y] : null
		};
		}
		else
		{
			neighbors = new List<HexagonGame>
		{
			(x-1 >= 0 && y-1 >= 0)               ? gameHexagons[x-1, y - 1] : null,
			(y-1 >= 0)                           ? gameHexagons[x, y-1] : null,
			(x+1 < gridSize.x)                   ? gameHexagons[x+1, y] : null,
			(y+1 < gridSize.y)                   ? gameHexagons[x, y+1] : null,
			(x-1 >= 0 && y+1 < gridSize.y)       ? gameHexagons[x-1, y+1] : null,
			(x-1 >= 0)                           ? gameHexagons[x-1, y] : null
		};
		}

		// Remove null values
		return neighbors.Where(n => n != null).ToList();
	}


	public void ForestGen()
	{
		foreach (Hexagon plain in plainHexagons)
		{
			// Generate a Perlin noise value
			float xCoord = plain.coordinates.x * noiseSeedForest;
			float yCoord = plain.coordinates.y * noiseSeedForest;
			float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

			// Determine tile type based on noise value
			if (noiseValue > 0.70f)
			{
				plain.hexType = forestHex;
			}
		}
	}


	private void InitializePlayers()
	{
		player = new Player("Tyurn", Color.red);
		enemy = new Player("Undeads", Color.black);
	}

	private void SpawnCitiesAndUnits()
	{
		HexagonGame playerCityHex = possibleSpawnHexes[Random.Range(0, possibleSpawnHexes.Count)];
		List<HexagonGame> enemySpawnHexes = new List<HexagonGame>();
		List<HexagonGame> playerUnitSpawn = new List<HexagonGame>();
		List<HexagonGame> enemyUnitSpawn = new List<HexagonGame>();

		foreach (HexagonGame hex in possibleSpawnHexes)
		{
			if (HexDistance(hex.coordinates, playerCityHex.coordinates) > 5)
			{
				enemySpawnHexes.Add(hex);
			}
		}
		HexagonGame enemyCityHex = enemySpawnHexes[Random.Range(0, enemySpawnHexes.Count)];

		GameObject playerCityLocal = Instantiate(playerCityModel, playerCityHex.transform.position, Quaternion.identity);
		playerCityHex.tag = "City";
		playerCityHex.gameObject.AddComponent<City>();
		playerCity = playerCityHex.gameObject.GetComponent<City>();
		playerCity.gameHex = playerCityHex;
		playerCity.owner = player;
		playerCity.ctrl = this;

		DestroyImmediate(playerCityModel.GetComponent<BoxCollider>(), true);

		GameObject enemyCityLocal = Instantiate(enemyCityModel, enemyCityHex.transform.position, Quaternion.identity);
		enemyCityHex.tag = "EnemyCity";
		enemyCityHex.gameObject.AddComponent<City>();
		enemyCity = enemyCityHex.gameObject.GetComponent<City>();
		enemyCity.gameHex = enemyCityHex;
		enemyCity.owner = enemy;
		enemyCity.ctrl = this;


		DestroyImmediate(enemyCityModel.GetComponent<BoxCollider>(), true);


		playerCityLocal.transform.localScale = new Vector3(7, 7, 7);
		enemyCityLocal.transform.localScale = new Vector3(7, 7, 7);
		Vector3 pos = playerCityLocal.transform.position;
		pos.y = (float)(pos.y + 0.4);
		playerCityLocal.transform.position = pos;

		pos = enemyCityLocal.transform.position;
		pos.y = (float)(pos.y + 0.6);
		enemyCityLocal.transform.position = pos;

		List<HexagonGame> neighbors = GetGameNeighbors(playerCityHex);
		for (int i = 0; i < neighbors.Count; i++)
		{
			if (neighbors[i].hexType.Equals(waterHex))
			{
				neighbors.RemoveAt(i);
				i--;
			}
		}
		for (int i = 0; i < STARTING_UNITS; i++)
		{
			HexagonGame chosenHex = neighbors[Random.Range(0, neighbors.Count)];
			pos = chosenHex.rawPosition;
			pos.y = 2.36f;
			GameObject playerUnit = Instantiate(playerUnitModel, pos, Quaternion.identity);

			Unit unitComp = playerUnit.GetComponent<Unit>();
			chosenHex.tag="Occupied";
			unitComp.coordinates = chosenHex.coordinates;
			unitComp.owner = player;
			unitComp.atk = STARTING_UNIT_ATK;
			unitComp.def = STARTING_UNIT_DEF;
			unitComp.maxHp = STARTING_UNIT_MAXHP;
			unitComp.hp = STARTING_UNIT_MAXHP;
			player.units.Add(playerUnit.GetComponent<Unit>());
			neighbors.Remove(chosenHex);
		}

		neighbors = GetGameNeighbors(enemyCityHex);
		for (int i = 0; i < neighbors.Count; i++)
		{
			if (neighbors[i].hexType.Equals(waterHex))
			{
				neighbors.RemoveAt(i);
				i--;
			}
		}
		for (int i = 0; i < STARTING_UNITS; i++)
		{
			HexagonGame chosenHex = neighbors[Random.Range(0, neighbors.Count)];
			pos = chosenHex.rawPosition;
			pos.y = 2f;
			GameObject enemyUnit = Instantiate(enemyUnitModel, pos, Quaternion.identity);

			Unit unitComp = enemyUnit.GetComponent<Unit>();
			chosenHex.tag="Occupied";
			unitComp.coordinates = chosenHex.coordinates;
			unitComp.owner = enemy;
			unitComp.atk = STARTING_UNIT_ATK;
			unitComp.def = STARTING_UNIT_DEF;
			unitComp.maxHp = STARTING_UNIT_MAXHP;
			unitComp.hp = STARTING_UNIT_MAXHP;
			enemy.units.Add(unitComp);
			neighbors.Remove(chosenHex);
		}
	}

	int HexDistance(Vector2Int a, Vector2Int b)
	{
		// Convert the first hex from offset to axial coordinates
		Vector3Int a_axial = OffsetToAxial(a);
		// Convert the second hex from offset to axial coordinates
		Vector3Int b_axial = OffsetToAxial(b);

		// Compute the axial distance between the two hexes
		return (Mathf.Abs(a_axial.x - b_axial.x)
				+ Mathf.Abs(a_axial.y - b_axial.y)
				+ Mathf.Abs(a_axial.z - b_axial.z)) / 2;
	}

	Vector3Int OffsetToAxial(Vector2Int offset)
	{
		int col = offset.x;
		int row = offset.y;

		// Convert odd-r offset to axial coordinates
		int x = col;
		int z = row - (col - (col & 1)) / 2;
		int y = -x - z;

		return new Vector3Int(x, y, z);
	}

	public bool SpawnUnit(Player owner)
	{
		City city;
		if (owner == player)
			city = playerCity;
		else
			city = enemyCity;


		List<HexagonGame> neighbors = GetGameNeighbors(city.gameHex);
		for (int i = 0; i < neighbors.Count; i++)
		{
			if (neighbors[i].hexType.Equals(waterHex))
			{
				neighbors.RemoveAt(i);
				i--;
			}
			else
			{
				foreach (Unit unit in owner.units)
				{
					if (unit.coordinates == neighbors[i].coordinates)
					{
						neighbors.RemoveAt(i);
						i--;
						break;
					}
				}
			}
		}
		if (neighbors.Count > 0)
		{
			//TODO rendere queste delle variabili (3 , 2 , 5)
			int atk = STARTING_UNIT_ATK + owner.unitUpgradeLevel * 3;
			int def = STARTING_UNIT_DEF + owner.unitUpgradeLevel * 2;
			int hp = STARTING_UNIT_MAXHP + owner.unitUpgradeLevel * 5;
			int mov = 3 + owner.unitUpgradeLevel;

			HexagonGame chosenHex = neighbors[Random.Range(0, neighbors.Count)];
			Vector3 pos = chosenHex.rawPosition;
			pos.y = 2.36f;
			GameObject playerUnit = Instantiate(playerUnitModel, pos, Quaternion.identity);

			Unit unitComp = playerUnit.GetComponent<Unit>();
			unitComp.coordinates = chosenHex.coordinates;
			unitComp.owner = player;
			unitComp.atk = atk;
			unitComp.def = def;
			unitComp.maxHp = hp;
			unitComp.hp = hp;
			unitComp.movementUnits = mov;
			player.units.Add(playerUnit.GetComponent<Unit>());

			return true;
		}

		return false;
	}
}

