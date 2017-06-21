using System.Collections;
using System.Collections.Generic;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using System.IO;
using Excel;
using System.Data;
using UnityEngine;

public class ExcelManager : MonoBehaviour {
	private LocalisationManager _localMgr;

	// Use this for initialization
	void Start () {
		_localMgr = LocalisationManager.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
	}

	//
	public void ExportAllExcels() {
		_localMgr = GetComponent<LocalisationManager>();
		foreach(Language l in _localMgr.languages) {
			Debug.Log("Exporting " + l.name + " as .xls");
			ExportLocalizationExcel(l, _localMgr.template);
		}
	}

	//
	public void ImportAllExcels() {
		_localMgr = GetComponent<LocalisationManager>();
		foreach(Language l in _localMgr.languages) {
			//ImportLocalizationExcel(l, _localMgr.template);
		}
	}

	//
	public void ExportLocalizationExcel(Language language, Language template) {
		FileStream filePath =
			new FileStream(CreatePath(language.name), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
		HSSFWorkbook excelFile = new HSSFWorkbook();
		
		for(int vgIndex = 0; vgIndex < language.groups.Count; vgIndex++) {
			Language.ValueGroup vg = language.groups[vgIndex];
			HSSFSheet excelSheet = (HSSFSheet)excelFile.CreateSheet(vg.title);

			HSSFRow firstRow = excelSheet.CreateRow(0) as HSSFRow;
			WriteToCell(firstRow, 0, "Key Value");
			WriteToCell(firstRow, 1, template.name);
			WriteToCell(firstRow, 2, language.name);
			int r = 1;

			for(int kvIndex = 0; kvIndex < vg.values.Count; kvIndex++) {
				Language.KeyValue kv = vg.values[kvIndex];
				HSSFRow kvRow = excelSheet.CreateRow(r++) as HSSFRow;
				WriteToCell(kvRow, 0, kv.key);
				WriteToCell(kvRow, 1, template.GetValue(vg.title, kv.key));
				WriteToCell(kvRow, 2, kv.value);
				for(int v = 0; v < kv.values.Count; v++) {
					HSSFRow vRow = excelSheet.CreateRow(r++) as HSSFRow;
					WriteToCell(vRow, 1, template.GetValue(vg.title, kv.key, v));
					WriteToCell(vRow, 2, kv.values[v]);
				}
			}
			r++;
		}

		excelFile.Write(filePath);
		excelFile.Close();
	}

	//
	public void ImportLocalizationExcel(Language language) {

	}

	//
	public string CreatePath(string fileName) {
		return Application.dataPath + "/Data/Localisation/" + fileName + ".xls";
	}

	//
	public void WriteToCell(HSSFSheet sheet, int r, int c, string value, HSSFFont font) {
		HSSFRow row = sheet.CreateRow(r) as HSSFRow;
		HSSFCell cell = row.CreateCell(c) as HSSFCell;
		cell.SetCellValue(value);
		cell.CellStyle.SetFont(font);
	}

	//
	public void WriteToCell(HSSFSheet sheet, int r, int c, string value) {
		HSSFRow row = sheet.CreateRow(r) as HSSFRow;
		HSSFCell cell = row.CreateCell(c) as HSSFCell;
		cell.SetCellValue(value);
	}

	//
	public void WriteToCell(HSSFRow row, int c, string value) {
		HSSFCell cell = row.CreateCell(c) as HSSFCell;
		cell.SetCellValue(value);
	}

	//
	public HSSFFont GetHeaderFont(HSSFWorkbook file) {
		HSSFFont font = file.CreateFont() as HSSFFont;
		font.FontName = "Tahoma";
		font.FontHeightInPoints = 14;
		font.Color = HSSFColor.Gold.Index;
		font.Boldweight = (short)FontBoldWeight.Bold;
		return font;
	}

	//
	public HSSFFont GetBodyFont(HSSFWorkbook file) {
		HSSFFont font = file.CreateFont() as HSSFFont;
		font.FontName = "Tahoma";
		font.FontHeightInPoints = 14;
		font.Color = HSSFColor.Gold.Index;
		font.Boldweight = (short)FontBoldWeight.Bold;
		return font;
	}

	//
	public HSSFFont CreateFont(HSSFWorkbook file, string fontStyle, short fontSize, short color, short bold) {
		HSSFFont font = file.CreateFont() as HSSFFont;
		font.FontName = fontStyle;
		font.FontHeightInPoints = fontSize;
		font.Color = color;
		font.Boldweight = bold;
		return font;
	}
}
