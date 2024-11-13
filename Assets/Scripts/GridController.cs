using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridController : MonoBehaviour
{
	public List<Hexagon> waterHexagons = new();
	public List<Hexagon> mountainHexagons = new();
	public List<Hexagon> plainHexagons = new();
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
	public List<HexagonGame> possibleSpawnHexes = new();

	// Constants
	public int STARTING_UNITS = 2;
	public int STARTING_UNIT_ATK = 6;
	public int STARTING_UNIT_DEF = 4;
	public int STARTING_UNIT_MAXHP = 10;

	public int STARTING_CITY_HP = 50;
	public int STARTING_CITY_DEFENSE = 6;
	public int DEFAULT_FUNDS_PER_TURN = 50;

	public int TRAIN_UNIT_COST = 250;
	public int CITY_UPGRADE_COST = 300;
	public int UNIT_UPGRADE_COST = 400;
	public int UNIT_ATK_UPGRADE = 4;
	public int UNIT_DEF_UPGRADE = 2;
	public int UNIT_HP_UPGRADE = 5;
	public int CITY_DEF_UPGRADE = 2;
	public int CITY_HP_UPGRADE = 5;
	public int FUNDS_PER_HEX = 10;
	public int FERTILE_BONUS_FUNDS = 5;
	public int FUNDS_PER_HEX_UPGRADE = 2;
	public int FUNDS_PER_TURN_UPGRADE = 20;

	public int MAX_ENEMY_UNITS = 6;

	public Hexagon[,] hexagons;
	public HexagonGame[,] gameHexagons;

	// Noise parameters
	public float noiseSeedAlt;
	public float noiseSeedForest;


	public Vector3 scale = new(9.98f, 10, 8.66f);
	public float hexSizeX = 9.98f;
	public float hexSizeY = 8.66f;

	public Pathfinding pathfinder;

	public Player player;

	public Player enemy;
	public City playerCity;

	public City enemyCity;

	public TurnManager turnManager;
	public GeneralHUDController HUDctrl;
	public Canvas gameHUDcanvas;
	public ParticleSystem deathParticle;
	public ParticleSystem spawnParticle;
	public ParticleSystem cityAttackParticle;
	public ParticleSystem cityDestroyedParticle;

	private bool firstUpdate = false;

	// Start is called before the first frame update
	void Start()
	{
		ownerOverlay = ownerOverlayLoad;
		hexagons = new Hexagon[gridSize.x, gridSize.y]; // Initializes the grid for base hexagons
		gameHexagons = new HexagonGame[gridSize.x, gridSize.y]; // Initializes the grid for game hexagon objects
		noiseSeedAlt = Random.Range(0.1f, 0.5f); // Random seed for altitude noise
		noiseSeedForest = Random.Range(0.1f, 0.5f); // Random seed for forest generation noise
		GenerateHexMap(gridSize); // Generates the hex map layout based on grid size

		// Identifies possible spawn locations
		foreach (HexagonGame gameHex in gameHexagons)
		{
			if (gameHex.CompareTag("MovableTerrain") && GetGameNeighbors(gameHex).Count == 6)
			{
				possibleSpawnHexes.Add(gameHex);
			}
		}

		InitializePlayers(); // Sets up players in the game
		SpawnCitiesAndUnits(); // Spawns initial cities and units

		// Finds the TurnManager and HUD controller objects in the scene
		turnManager = FindObjectOfType<TurnManager>();
		HUDctrl = FindObjectOfType<GeneralHUDController>();

		firstUpdate = true; // Flags for initial update
	}

	// Update is called once per frame
	void Update()
	{
		if (firstUpdate)
		{
			// Claims neighboring hexes around the player's and enemy's cities
			List<HexagonGame> neighbors = GetGameNeighbors(playerCity.gameHex);
			for (int i = 0; i < neighbors.Count; i++)
			{
				if (neighbors[i].hexType.Equals(waterHex))
				{
					neighbors.RemoveAt(i); // Removes water hexes from neighbors
					i--;
				}
				else
				{
					neighbors[i].ClaimHex(player); // Claims hex for the player
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
					neighbors[i].ClaimHex(enemy); // Claims hex for the enemy
				}
			}
			firstUpdate = false; // Ensures this block only runs once
		}
	}

	// Generates the hex map based on grid size and assigns hex types
	public void GenerateHexMap(Vector2Int gridSize)
	{
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				Hexagon hex = new(new Vector2Int(x, y));
				hex.rawPosition = GetPosForHexFromCoord(hex.coordinates); // Calculates hex position
				hex.hexType = DetermineTileMainType(hex.coordinates); // Sets type based on terrain
				if (hex.hexType.Equals(waterHex))
					waterHexagons.Add(hex);
				else if (hex.hexType.Equals(plainHex))
					plainHexagons.Add(hex);
				else if (hex.hexType.Equals(mountainHex))
					mountainHexagons.Add(hex);

				hexagons[x, y] = hex; // Adds hex to grid
			}
		}

		ForestGen(); // Generates forests in certain plains

		hexagons = SmoothGen(hexagons); // Smooths terrain transitions
		HexRenderer(hexagons); // Renders hexagons visually

		// Transfers hex type data to game objects
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				Hexagon hex = hexagons[x, y];
				HexagonGame gameHex = gameHexagons[x, y];
				gameHex.hexType = hex.hexType;
			}
		}
		pathfinder = new Pathfinding(this, gameHexagons, gridSize.x, gridSize.y); // Initializes pathfinding
	}

	// Converts hex coordinates to world position for rendering
	public Vector3 GetPosForHexFromCoord(Vector2Int coordinates)
	{
		float xOffset = hexSizeX; // Horizontal offset for staggered columns
		float yOffset = hexSizeY * 0.866f; // Vertical offset based on hex size

		float posX = coordinates.x * xOffset;
		float posY = coordinates.y * yOffset;

		if (coordinates.y % 2 == 1) // Stagger for every other row
		{
			posX += hexSizeX * 0.5f;
		}

		return new Vector3(posX, 0, -posY); // Returns calculated position
	}

	// Determines the main type of a tile based on Perlin noise (for varied terrain)
	public GameObject DetermineTileMainType(Vector2Int coords)
	{
		float xCoord = coords.x * noiseSeedAlt;
		float yCoord = coords.y * noiseSeedAlt;
		float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

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

	// Applies smoothing to the generated terrain to avoid abrupt changes
	public Hexagon[,] SmoothGen(Hexagon[,] hexes)
	{
		int[,] countWaterHexs = new int[gridSize.x, gridSize.y];
		int[,] countPlainHexs = new int[gridSize.x, gridSize.y];
		int[,] countMountainHexs = new int[gridSize.x, gridSize.y];

		// Adjusts water-to-plain transitions based on neighboring hexes
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				foreach (Hexagon neighbour in GetNeighbors(hexes[x, y]))
				{
					if (neighbour != null && neighbour.hexType.Equals(waterHex))
						countWaterHexs[x, y]++;
				}
				if (hexes[x, y].hexType.Equals(waterHex) && countWaterHexs[x, y] <= 0)
					hexes[x, y].hexType = plainHex; // Converts isolated water tiles to plain
			}
		}

		// Adjusts plain-to-fertile plain transitions based on nearby water tiles
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				foreach (Hexagon neighbour in GetNeighbors(hexes[x, y]))
				{
					if (neighbour != null && neighbour.hexType.Equals(waterHex))
						countWaterHexs[x, y]++;
				}
				if (hexes[x, y].hexType.Equals(plainHex) && countWaterHexs[x, y] >= 2)
					hexes[x, y].hexType = fertilePlainHex;
			}
		}
		return hexes;
	}

	// Renders each hexagon in the scene and adds necessary components
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
					newHex.tag = "MovableTerrain"; // Tags non-water hexes as movable terrain

				gameHexagons[x, y] = newHex.GetComponent<HexagonGame>(); // Adds to game hex grid
			}
		}
	}

	// Returns a list of neighboring hexes for a given hex
	public List<Hexagon> GetNeighbors(Hexagon hex)
	{
		int x = hex.coordinates.x;
		int y = hex.coordinates.y;

		// Adjusts neighboring positions based on row stagger pattern
		if (y % 2 == 1)
		{
			return new List<Hexagon>
		{
			(y-1>=0) ? hexagons[x,y - 1] : null,
			(x+1<gridSize.x && y-1>=0) ? hexagons[x+1,y-1] : null,
			(x+1<gridSize.x) ? hexagons[x+1,y] : null,
			(x+1<gridSize.x && y+1<gridSize.y) ? hexagons[x+1,y+1] : null,
			(y+1<gridSize.y) ? hexagons[x,y+1] : null,
			(x-1>=0) ? hexagons[x-1,y] : null
		};
		}
		else
		{
			return new List<Hexagon>
		{
			(x-1>=0 && y-1>=0) ? hexagons[x-1,y - 1] : null,
			(y-1>=0) ? hexagons[x,y-1] : null,
			(x+1<gridSize.x) ? hexagons[x+1,y] : null,
			(y+1<gridSize.y) ? hexagons[x,y+1] : null,
			(x-1>=0) ? hexagons[x-1,y] : null,
			(x-1>=0 && y+1<gridSize.y) ? hexagons[x-1,y+1] : null
		};
		}
	}

	// Returns a list of neighboring game hexes for a given hex
	public List<HexagonGame> GetGameNeighbors(HexagonGame hex)
	{
		int x = hex.coordinates.x;
		int y = hex.coordinates.y;

		var neighbors = new List<HexagonGame>();

		// Adjusts neighboring positions based on row stagger pattern
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


	// Generates forests on plains based on Perlin noise values
	public void ForestGen()
	{
		foreach (Hexagon plain in plainHexagons)
		{
			// Generates a Perlin noise value based on coordinates
			float xCoord = plain.coordinates.x * noiseSeedForest;
			float yCoord = plain.coordinates.y * noiseSeedForest;
			float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

			// Sets hex type to forest if noise value exceeds threshold
			if (noiseValue > 0.70f)
			{
				plain.hexType = forestHex;
			}
		}
	}

	// Initializes player and enemy with unique names and colors
	private void InitializePlayers()
	{
		player = new Player("Player", Color.blue); // Player with blue color
		enemy = new Player("Undeads", Color.black); // Enemy with black color

		// Sets controller references for player and enemy
		player.ctrl = this;
		enemy.ctrl = this;
	}

	// Spawns cities and initial units for both players
	private void SpawnCitiesAndUnits()
	{
		// Randomly selects a spawn location for player city
		HexagonGame playerCityHex = possibleSpawnHexes[Random.Range(0, possibleSpawnHexes.Count)];
		List<HexagonGame> enemySpawnHexes = new();

		// Finds potential spawn locations for enemy city, ensuring distance from player city
		foreach (HexagonGame hex in possibleSpawnHexes)
		{
			if (HexDistance(hex.coordinates, playerCityHex.coordinates) > 8) 
			{
				enemySpawnHexes.Add(hex);
			}
		}
		// Randomly selects an enemy city spawn from eligible hexes
		HexagonGame enemyCityHex = enemySpawnHexes[Random.Range(0, enemySpawnHexes.Count)];

		// Instantiates player city and configures its properties
		GameObject playerCityLocal = Instantiate(playerCityModel, playerCityHex.transform.position, Quaternion.identity);
		playerCityHex.tag = "PlayerCity";
		playerCityHex.gameObject.AddComponent<City>();
		playerCity = playerCityHex.gameObject.GetComponent<City>();
		playerCity.gameHex = playerCityHex;
		playerCity.owner = player;
		playerCity.ctrl = this;

		// Removes collision box to avoid interference
		DestroyImmediate(playerCityModel.GetComponent<BoxCollider>(), true);

		// Instantiates enemy city and configures its properties
		GameObject enemyCityLocal = Instantiate(enemyCityModel, enemyCityHex.transform.position, Quaternion.identity);
		enemyCityHex.tag = "EnemyCity";
		enemyCityHex.gameObject.AddComponent<City>();
		enemyCity = enemyCityHex.gameObject.GetComponent<City>();
		enemyCity.gameHex = enemyCityHex;
		enemyCity.owner = enemy;
		enemyCity.ctrl = this;

		// Removes collision box from enemy city as well
		DestroyImmediate(enemyCityModel.GetComponent<BoxCollider>(), true);

		// Sets scale and adjusts position for both cities
		playerCityLocal.transform.localScale = new Vector3(7, 7, 7);
		enemyCityLocal.transform.localScale = new Vector3(7, 7, 7);
		Vector3 pos = playerCityLocal.transform.position;
		pos.y = (float)(pos.y + 0.4);
		playerCityLocal.transform.position = pos;

		pos = enemyCityLocal.transform.position;
		pos.y = (float)(pos.y + 0.6);
		enemyCityLocal.transform.position = pos;

		// Claims neighboring hexes around player city for spawning units
		List<HexagonGame> neighbors = GetGameNeighbors(playerCityHex);
		for (int i = 0; i < neighbors.Count; i++)
		{
			if (neighbors[i].hexType.Equals(waterHex))
			{
				neighbors.RemoveAt(i);
				i--;
			}
		}

		// Spawns initial units for player in neighboring hexes
		for (int i = 0; i < STARTING_UNITS; i++)
		{
			HexagonGame chosenHex = neighbors[Random.Range(0, neighbors.Count)];
			pos = chosenHex.rawPosition;
			pos.y = 2.36f;
			GameObject playerUnit = Instantiate(playerUnitModel, pos, Quaternion.identity);
			Unit unitComp = playerUnit.GetComponent<Unit>();
			chosenHex.tag = "Occupied";
			unitComp.coordinates = chosenHex.coordinates;
			unitComp.owner = player;
			unitComp.atk = STARTING_UNIT_ATK;
			unitComp.def = STARTING_UNIT_DEF;
			unitComp.maxHp = STARTING_UNIT_MAXHP;
			unitComp.hp = STARTING_UNIT_MAXHP;
			player.units.Add(playerUnit.GetComponent<Unit>());
			neighbors.Remove(chosenHex);
		}

		// Claims neighboring hexes around enemy city for spawning units
		neighbors = GetGameNeighbors(enemyCityHex);
		for (int i = 0; i < neighbors.Count; i++)
		{
			if (neighbors[i].hexType.Equals(waterHex))
			{
				neighbors.RemoveAt(i);
				i--;
			}
		}

		// Spawns initial units for enemy in neighboring hexes
		for (int i = 0; i < STARTING_UNITS; i++)
		{
			HexagonGame chosenHex = neighbors[Random.Range(0, neighbors.Count)];
			pos = chosenHex.rawPosition;
			pos.y = 2f;
			GameObject enemyUnit = Instantiate(enemyUnitModel, pos, Quaternion.identity);
			Unit unitComp = enemyUnit.GetComponent<Unit>();
			chosenHex.tag = "Occupied";
			unitComp.coordinates = chosenHex.coordinates;
			unitComp.owner = enemy;
			unitComp.atk = STARTING_UNIT_ATK;
			unitComp.def = STARTING_UNIT_DEF;
			unitComp.maxHp = STARTING_UNIT_MAXHP;
			unitComp.hp = STARTING_UNIT_MAXHP;
			enemy.units.Add(unitComp);
			neighbors.Remove(chosenHex);
		}

		// Assigns city references to both player and enemy
		player.city = playerCity;
		enemy.city = enemyCity;
	}

	// Calculates distance between two hexes based on their axial coordinates
	int HexDistance(Vector2Int a, Vector2Int b)
	{
		// Converts hexes's offset coordinates to axial coordinates
		Vector3Int a_axial = OffsetToAxial(a);
		Vector3Int b_axial = OffsetToAxial(b);

		// Calculates the axial distance between the two hexes
		return (Mathf.Abs(a_axial.x - b_axial.x)
				+ Mathf.Abs(a_axial.y - b_axial.y)
				+ Mathf.Abs(a_axial.z - b_axial.z)) / 2;
	}

	// Converts hex coordinates from offset to axial format for distance calculation
	Vector3Int OffsetToAxial(Vector2Int offset)
	{
		int col = offset.x;
		int row = offset.y;

		// Adjusts coordinates for odd-row offset to axial coordinates
		int x = col;
		int z = row - (col - (col & 1)) / 2;
		int y = -x - z;

		return new Vector3Int(x, y, z);
	}

	// Spawns a new unit for the specified player in an unoccupied neighboring hex
	public bool SpawnUnit(Player owner)
	{
		// Finds unoccupied neighboring hexes around the player’s city
		List<HexagonGame> neighbors = GetGameNeighbors(owner.city.gameHex);
		for (int i = 0; i < neighbors.Count; i++)
		{
			if (neighbors[i].hexType.Equals(waterHex) || neighbors[i].CompareTag("Occupied"))
			{
				neighbors.RemoveAt(i);
				i--;
			}
		}
		if (neighbors.Count > 0)
		{
			// Determines unit stats, accounting for player upgrades
			int atk = STARTING_UNIT_ATK + owner.unitUpgradeLevel * UNIT_ATK_UPGRADE;
			int def = STARTING_UNIT_DEF + owner.unitUpgradeLevel * UNIT_DEF_UPGRADE;
			int hp = STARTING_UNIT_MAXHP + owner.unitUpgradeLevel * UNIT_HP_UPGRADE;
			int mov = 3 + owner.unitUpgradeLevel;

			// Selects a random hex for spawning the unit
			HexagonGame chosenHex = neighbors[Random.Range(0, neighbors.Count)];
			chosenHex.tag = "Occupied";
			chosenHex.ClaimHex(owner);
			Vector3 pos = chosenHex.rawPosition;
			pos.y = 2.36f;
			GameObject unit;

			// Instantiates the unit model based on owner type (enemy or player)
			if (owner == enemy)
			{
				unit = Instantiate(enemyUnitModel, pos, Quaternion.identity);
			}
			else
			{
				unit = Instantiate(playerUnitModel, pos, Quaternion.identity);
			}
			Instantiate(spawnParticle, chosenHex.transform.position, Quaternion.Euler(-90, 0, 0));

			// Sets unit properties and assigns it to the owner's unit list
			Unit unitComp = unit.GetComponent<Unit>();
			unitComp.coordinates = chosenHex.coordinates;
			unitComp.owner = owner;
			unitComp.atk = atk;
			unitComp.def = def;
			unitComp.maxHp = hp;
			unitComp.hp = hp;
			unitComp.movementUnits = mov;
			owner.units.Add(unit.GetComponent<Unit>());

			return true; // Returns true if spawn was successful
		}

		return false; // Returns false if no spawn location was available
	}

}

