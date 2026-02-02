# COMPLETE IMPLEMENTATION GUIDE

## Step-by-Step Implementation

### Phase 1: Database Setup (Priority: HIGH)

1. **Update DatabaseHelper.cs**
   - Add call to `LandOwnerDatabaseSchema.CreateSchema(connection)` in `InitializeDatabase()`
   
2. **Test Database Creation**
   - Open a project
   - Verify tblLandOwner and tblOriginalLandParcels are created

### Phase 2: Import Manager Updates (Priority: HIGH)

1. **Replace existing frmImportManager files with:**
   - frmImportManager_Updated.cs (in outputs folder)
   - frmImportManager_Updated_Designer.cs (create from template)

2. **Key Features Implemented:**
   - Step 1: Browse → Load File → Select Sheet (ComboBox) → Import Data
   - Step 2: DataGridView mapping (not individual ComboBoxes)
   - Step 3: Context menu (Edit/Delete/Fix Error)
   - Step 4: Passive validation + incremental tracking
   - Save directly to database with deduplication

3. **Integration in Main Form:**
```csharp
private void importToolStripMenuItem_Click(object sender, EventArgs e)
{
    if (!CurrentProject.IsOpen)
    {
        MessageBox.Show("Please open or create a project first.", "No Project",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    using (var importForm = new frmImportManager(CurrentProject.Info.ProjectPath))
    {
        if (importForm.ShowDialog() == DialogResult.OK)
        {
            MessageBox.Show($"Successfully imported {importForm.ImportedCount} records!",
                "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
```

### Phase 3: View/Edit Records Manager (Priority: HIGH)

1. **Create frmLandownerRecordsManager** (matches Image 2)
   - Toolbar: Add, Edit, Delete, Find Duplicates, Search
   - DataGridView with all parcel data
   - Pagination controls
   - "View Details / Attach Documents" button opens detail form

2. **Integration in Main Form:**
```csharp
private void viewEditRecordsToolStripMenuItem_Click(object sender, EventArgs e)
{
    if (!CurrentProject.IsOpen)
    {
        MessageBox.Show("Please open or create a project first.", "No Project",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    var viewForm = new frmLandownerRecordsManager(CurrentProject.Info.ProjectPath);
    viewForm.ShowDialog();
}
```

### Phase 4: Landowner Details Form (Priority: HIGH)

1. **Create frmLandownerDetails** (matches Image 1)
   - Picture box + Upload Photo button
   - Editable fields
   - Attach Documents section
   - Save Changes / Close buttons

2. **File Management:**
   - Photos: `ProjectPath/Images/LandOwners Photos/{LandOwnersName}_{CitizenshipNo}/photo.jpg`
   - Documents: `ProjectPath/Documents/LandOwner_{OwnerId}/`
   - Save paths to database

### Phase 5: Testing Checklist

- [ ] Create new project
- [ ] Import Excel file (multiple sheets)
- [ ] Select correct sheet
- [ ] Auto-map fields
- [ ] Validate records
- [ ] See invalid rows in light red
- [ ] Edit invalid record via context menu
- [ ] Delete multiple records
- [ ] Save to database
- [ ] Verify unique owners extracted
- [ ] Open Records Manager
- [ ] Search/filter records
- [ ] View details form
- [ ] Upload photo
- [ ] Attach documents
- [ ] Save changes
- [ ] Verify file paths in database

---

## File Organization

### Replace These Files:
1. `ExcelImportService.cs` → Replace with Enhanced version
2. `frmImportManager.cs` + Designer → Replace with Updated version

### Add These New Files:
1. `LandOwnerDatabaseSchema.cs` - Database schema
2. `LandOwnerModels.cs` - LandOwner & OriginalLandParcel models
3. `LandOwnerRepository.cs` - Database operations (enhanced)
4. `frmLandownerRecordsManager.cs` + Designer - View/Edit form
5. `frmLandownerDetails.cs` + Designer - Detail form with photo/documents

### Modify These Files:
1. `DatabaseHelper.cs` - Add schema initialization call
2. `Main_form.cs` - Add menu items and handlers

---

## Database Schema

```sql
-- Unique landowners
CREATE TABLE tblLandOwner (
    LandOwnerId INTEGER PRIMARY KEY AUTOINCREMENT,
    LandOwnersName TEXT NOT NULL,
    FatherSpouse TEXT,
    Gender TEXT,
    CitizenshipNumber TEXT,
    Address TEXT,
    PhotoPath TEXT,
    DocumentsFolderPath TEXT,
    CreatedDate TEXT,
    ModifiedDate TEXT,
    UNIQUE(LandOwnersName, FatherSpouse, CitizenshipNumber)
);

-- Parcels with FK to owner
CREATE TABLE tblOriginalLandParcels (
    ParcelId INTEGER PRIMARY KEY AUTOINCREMENT,
    LandOwnerId INTEGER NOT NULL,
    ParcelNo TEXT NOT NULL,
    Province TEXT,
    District TEXT,
    MunicipalityVillage TEXT,
    MapSheetNo TEXT NOT NULL,
    IsTenant TEXT,
    LandUse TEXT,
    AreaInSqm REAL,
    AreaInRAPD TEXT,
    AreaInBKD TEXT,
    MothNo TEXT,
    PaanaNo TEXT,
    Remarks TEXT,
    ImportedDate TEXT,
    ModifiedDate TEXT,
    IsValid INTEGER DEFAULT 1,
    ValidationErrors TEXT,
    FOREIGN KEY (LandOwnerId) REFERENCES tblLandOwner(LandOwnerId),
    UNIQUE(ParcelNo, MapSheetNo)
);
```

---

## Menu Structure in Main Form

```
Data Menu
├── Land Owners Record
    ├── Import                          → frmImportManager
    └── View/Edit Records               → frmLandownerRecordsManager
```

---

## Key Performance Features

1. **Passive Validation**
   - Runs once after import
   - Doesn't run on every edit
   - User triggers manual re-validation

2. **Incremental Error Tracking**
   - Track deleted row indexes
   - Track edited row indexes
   - Update error list without full re-validation

3. **Context Menu**
   - Right-click on grid
   - Edit (single selection only)
   - Delete (multiple selection allowed)
   - Fix Error (only on invalid rows)

4. **Subtle Color Coding**
   - Invalid rows: #FFF5F5 (very light red)
   - Invalid text: #8B0000 (dark red)

---

## Common Issues & Solutions

### Issue: Import Manager doesn't open
**Solution:** Ensure project is open first (check Current Project.IsOpen)

### Issue: Sheet selection doesn't work
**Solution:** Verify ExcelImportService.LoadExcelFile() returns DataSet

### Issue: Validation is slow
**Solution:** Ensure passive validation is enabled (doesn't run on every change)

###Issue: Photos/documents don't save
**Solution:** Check folder creation permissions in project directory

### Issue: Duplicate owners not detected
**Solution:** Verify GetOwnerKey() uses correct fields (Name|FatherSpouse|Citizenship)

---

## Next Steps After Implementation

1. Test with small dataset (< 100 records)
2. Test with medium dataset (1,000 records)
3. Test with large dataset (10,000 records)
4. Measure validation performance
5. Test photo/document management
6. Test find duplicates feature
7. Add advanced filtering
8. Add export functionality

---

**Status:** Ready for implementation
**Priority Files:** Import Manager, Records Manager, Details Form
**Last Updated:** January 28, 2026
