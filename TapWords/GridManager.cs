using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
	[SerializeField] private GridLayoutGroup Grid;
	private DataManager DataManager;
	private int rows;
	private int columns;
	private HashSet<int> AvailableGridSlots = new HashSet<int>();
	private Dictionary<string, CrateData> OccupiedSlots = new Dictionary<string, CrateData>();
	public WordSearchResult WordSearchResult;
	private int placedSlotNum;

	private void Start()
	{
		columns = Grid.constraintCount;
		rows = Grid.transform.childCount / columns;
		SetGridInteractable(false);
	}

	public void Initialize()
	{
		DataManager = Singleton.Instance.DataManager;
		//populate AvailableGridSlots
		ClearGrid();
		//populate blocks
		if (Singleton.Instance.DataConfig.Blocks > 0)
		{
			for (int i = 0; i < Singleton.Instance.DataConfig.Blocks; i++)
			{
				int randomSlot = ChooseRandomSlot();
				AvailableGridSlots.Remove(randomSlot);
				CrateObj crateObj = Grid.transform.GetChild(randomSlot).GetComponent<CrateObj>();
				string key = GetCoordKey(crateObj.GetData().index);
				OccupiedSlots.Add(key, crateObj.GetData());
				crateObj.SetAsBlock();
			}
		}
		SetGridInteractable(false);
	}

	public int SlotCount()
	{
		return OccupiedSlots.Count;
	}

	public void ClearGrid ()
	{
		foreach (KeyValuePair<string, CrateData> kvp in OccupiedSlots)
		{
			CrateObj crate = Grid.transform.GetChild(kvp.Value.index).GetComponent<CrateObj>();
			crate.ClearCrate();
		}
		OccupiedSlots.Clear();
		int slotNum = 0;
		for (int i = 0; i < columns; i++)
		{
			for (int j = 0; j < rows; j++)
			{
				AvailableGridSlots.Add(slotNum);
				Grid.transform.GetChild(slotNum).GetComponent<CrateObj>().SetData(new CrateData(slotNum));
				slotNum++;
			}
		}
	}

	public bool SetLetterForSlot (LetterInfo letter, int slotNum)
	{
		CrateObj crate = Grid.transform.GetChild(slotNum).GetComponent<CrateObj>();
		crate.SetLetter(letter);
		
		//VFX for wil card
		if (letter.letter == "*") 
		{
			Singleton.Instance.VFXManager.PlayVFX(VFXItem.WILD_CARD, crate.transform.position);
		}
		//add item to OccupiedSlots
		
		OccupiedSlots.Add(GetCoordKey(slotNum), crate.GetData());
		RemoveSlot(slotNum);

		//set up for scoring check
		placedSlotNum = slotNum;
		int column = (slotNum) % columns;
		int row = (slotNum) / columns;

		return TryScoreWord(column, row);
	}
	
	public void SetSlotToCheat (int slotNum) 
	{
		UnhighlightGrid();
		CrateObj crate = Grid.transform.GetChild(slotNum).GetComponent<CrateObj>();
		crate.HighlightCrate();
		crate.SetTempInteractable();
	}
	
	public void CheatLetterForSlot (string letter, int slotNum) 
	{
		CrateObj crate = Grid.transform.GetChild(slotNum).GetComponent<CrateObj>();
		crate.CheatLetter(letter);
		UnhighlightGrid();
	}

	public bool TryPerformSwapAtItem (CrateObj sourceCrate)
	{
		bool didSwapAndScore = false;
		//determine grid coords in direction of modifier
		int column = (sourceCrate.GetData().index) % columns;
		int row = (sourceCrate.GetData().index) / columns;
		int origcolumn = column;
		int origrow = row;
		switch (sourceCrate.GetData().modifier)
		{
			case Modifier.LEFT:
				column--;
				break;
			case Modifier.TOP:
				row--;
				break;
			case Modifier.RIGHT:
				column++;
				break;
			case Modifier.BOTTOM:
				row++;
				break;
			default:
				break;
		}

		string key = column.ToString() + "," + row.ToString();
		//if there's an occupied slot, then perform swap
		if (OccupiedSlots.ContainsKey(key))
		{
			//swap it!
			CrateObj targetCrate = Grid.transform.GetChild(OccupiedSlots[key].index).GetComponent<CrateObj>();
			string sourceLetter = sourceCrate.GetData().letter;
			string targetLetter = targetCrate.GetData().letter;
			LetterInfo sourceInfo = new LetterInfo(targetLetter, targetCrate.GetData().modifier);
			sourceCrate.SetLetter(sourceInfo);
			targetCrate.SetLetter(new LetterInfo(sourceLetter, Modifier.NONE));
			didSwapAndScore = TryScoreWord(column, row, true);
			//also check where we swapped from
			didSwapAndScore = TryScoreWord(origcolumn, origrow, true);
			Singleton.Instance.VFXManager.PlayVFX(VFXItem.SWAPPER, targetCrate.transform.position);
		}
		return didSwapAndScore;
	}

	private bool TryScoreWord (int column, int row, bool isSwap = false)
	{
		WordSearchResult = CheckForWord(column, row, isSwap);

		//highlight winnning letters
		if (WordSearchResult.wasSuccessful)
		{
			WordSearchResult.scorePopupPosition = Grid.transform.GetChild(placedSlotNum).position;
			//swap the wild card if possible
			if (WordSearchResult.wildCardCrateIndex != -1)
			{
				Grid.transform.GetChild(WordSearchResult.wildCardCrateIndex).GetComponent<CrateObj>().SwapWildCardChar(WordSearchResult.wildCardReplacementLetter);
			}
			StartCoroutine(TriggerEffectsStaggered(WordSearchResult.CratesToClear));
			// foreach (int crateIndex in WordSearchResult.CratesToClear)
			// {
			// 	Grid.transform.GetChild(crateIndex).GetComponent<CrateObj>().HighlightCrate();
			// }
		}

		return WordSearchResult.wasSuccessful;
	}
	
	private IEnumerator TriggerEffectsStaggered(List<int> crateIndices)
{
	float delay = 0.1f; // Adjust the delay as needed

	foreach (int crateIndex in crateIndices)
	{
		Grid.transform.GetChild(crateIndex).GetComponent<TileFader>().TriggerEffect();
		yield return new WaitForSeconds(delay);
	}
}

	private string GetCoordKey (int slotNum)
	{
		int column = (slotNum) % columns;
		int row = (slotNum) / columns;
		return column.ToString() + "," + row.ToString();
	}

	public void SetGridInteractable (bool state)
	{
		for (int i = 0; i < Grid.transform.childCount; i++)
		{
			Grid.transform.GetChild(i).GetComponent<CrateObj>().SetInteractable(state);
		}
	}

	public void SetLettersInteractable(bool state)
	{
		foreach (KeyValuePair<string, CrateData> kvp in OccupiedSlots)
		{
			CrateObj crateObj = Grid.transform.GetChild(kvp.Value.index).GetComponent<CrateObj>();
			//any CrateObj with mods is always interactable
			crateObj.SetInteractable(state || crateObj.GetData().HasMods());
		}
	}
	
	public void SetGridTempInteractable () 
	{
		foreach (KeyValuePair<string, CrateData> kvp in OccupiedSlots)
		{
			CrateObj crate = Grid.transform.GetChild(kvp.Value.index).GetComponent<CrateObj>();
			crate.SetTempInteractable();
		}
	}
	
	public void ResetGridInteracable () 
	{
		foreach (KeyValuePair<string, CrateData> kvp in OccupiedSlots)
		{
			CrateObj crate = Grid.transform.GetChild(kvp.Value.index).GetComponent<CrateObj>();
			crate.ResetInteractable();
		}
	}
	
	public void UnhighlightGrid () 
	{
		foreach (KeyValuePair<string, CrateData> kvp in OccupiedSlots)
		{
			CrateObj crate = Grid.transform.GetChild(kvp.Value.index).GetComponent<CrateObj>();
			crate.UnhighlightCrate();
		}
	}
	
	public bool IsSlotOccupied (int index) 
	{
		return OccupiedSlots.ContainsKey(GetCoordKey(index));
	}

	public void DestroyCrate (int index)
	{
		CrateObj crateToDestroy = Grid.transform.GetChild(index).GetComponent<CrateObj>();
		Vector3 crateLocation = Grid.transform.GetChild(index).position;
		Singleton.Instance.VFXManager.PlayVFX(VFXItem.BOMB, crateLocation);
		crateToDestroy.ClearCrate();
		OccupiedSlots.Remove(GetCoordKey(index));
		AvailableGridSlots.Add(index);
	}

	private void RemoveSlot (int slotNum)
	{
		if (AvailableGridSlots.Count > 0)
		{
			AvailableGridSlots.Remove(slotNum);
		}
		else
		{
			Debug.LogError("Trying to remove slot from empty AvailableSlots");
		}
	}

	public int ChooseRandomSlot ()
	{
		int randomSlot = Random.Range(0, AvailableGridSlots.Count);
		int slotNum = AvailableGridSlots.ElementAt(randomSlot);
		return slotNum;
	}
	
	public int ChooseRandomOccupiedSlot ()
	{
		int randomSlot = Random.Range(0, OccupiedSlots.Count);
		 KeyValuePair<string, CrateData> kvp = OccupiedSlots.ElementAt(randomSlot);
		 int slotNum = kvp.Value.index;
		return slotNum;
	}

	public bool GridFull ()
	{
		return AvailableGridSlots.Count == 0;
	}

	public void DeleteLastWord ()
	{
		if (WordSearchResult.wasSuccessful)
		{
			foreach (int crateIndex in WordSearchResult.CratesToClear)
			{
				Grid.transform.GetChild(crateIndex).GetComponent<CrateObj>().ClearCrate();
				//remove from OccupiedSlots
				OccupiedSlots.Remove(GetCoordKey(crateIndex));
				//add back to AvailableGridSlots
				AvailableGridSlots.Add(crateIndex);
			}
		}
	}


	public WordSearchResult CheckForWord(int column, int row, bool isSwap = false)
	{
		//check for horizontal word - start to the left of column
		List<List<CrateData>> WordsToCheck = new List<List<CrateData>>();
		string key = column.ToString() + "," + row.ToString();
		List<CrateData> testCrates = new List<CrateData>();
		testCrates.Add(OccupiedSlots[key]);
		if (column > 0)
		{
			for (int i = column - 1; i >= 0; i--)
			{
				key = i.ToString() + "," + row.ToString();
				if (OccupiedSlots.ContainsKey(key) && !OccupiedSlots[key].IsBlock)
				{
					testCrates.Insert(0, OccupiedSlots[key]);
				}
				else
				{
					break;
				}
			}
		}
		//then to the right
		if (column < columns - 1)
		{
			for (int i = column + 1; i < columns; i++)
			{
				key = i.ToString() + "," + row.ToString();
				if (OccupiedSlots.ContainsKey(key) && !OccupiedSlots[key].IsBlock)
				{
					testCrates.Add(OccupiedSlots[key]);
				}
				else
				{
					break;
				}
			}
		}
		WordsToCheck.Add(testCrates);

		//check for vertical word - start to the top of row
		key = column.ToString() + "," + row.ToString();
		testCrates = new List<CrateData>();
		testCrates.Add(OccupiedSlots[key]);
		if (row > 0)
		{
			for (int i = row - 1; i >= 0; i--)
			{
				key = column.ToString() + "," + i.ToString();
				if (OccupiedSlots.ContainsKey(key) && !OccupiedSlots[key].IsBlock)
				{
					testCrates.Insert(0, OccupiedSlots[key]);
				}
				else
				{
					break;
				}
			}
		}

		//then down
		if (row < rows - 1)
		{
			for (int i = row + 1; i < rows; i++)
			{
				key = column.ToString() + "," + i.ToString();
				if (OccupiedSlots.ContainsKey(key) && !OccupiedSlots[key].IsBlock)
				{
					testCrates.Add(OccupiedSlots[key]);
				}
				else
				{
					break;
				}
			}
		}

		WordsToCheck.Add(testCrates);
		return DataManager.TryGetWord(WordsToCheck, new Vector2(column, row), isSwap);
	}

	public bool HasWildCard ()
	{
		bool hasWildCard = false;
		foreach (KeyValuePair<string, CrateData> kvp in OccupiedSlots)
		{
			if (kvp.Value.letter == "*") return true;
		}
		return hasWildCard;
	}
}
