# K-pop Shop Sample — UI Setup Guide

Checklist để build UI cho sample tại Unity Editor.

---

## 1. Prefabs cần tạo

### 1.1. `PopupNotice.prefab` (popup)
- Component: `PopupNotice` (kế thừa `BasePopup`)
- Cấu trúc:
  - Root: `CanvasGroup` / `Image` overlay mờ
  - `_backgroundBtn` (Button): Button full màn hình để click nền đóng popup (optional)
  - Panel chính:
    - `_txtTitle` (TMP_Text): tiêu đề
    - `_txtContent` (TMP_Text): nội dung, bật **Rich Text** để hiển thị `<b>...</b>`
    - `_btnClose` (Button): nút X đóng popup

### 1.2. `PopupBuyConfirm.prefab` (popup)
- Component: `PopupBuyConfirm` (kế thừa `BasePopup`)
- Cấu trúc:
  - `_backgroundBtn` (Button, optional)
  - Panel chính:
    - `_imgThumbnail` (Image): ảnh album
    - `_txtName` (TMP_Text): tên album
    - `_txtPrice` (TMP_Text): giá
    - `_btnConfirm` (Button): nút "Confirm / Buy"
    - `_btnCancel` (Button): nút "Cancel"
    - `_btnClose` (Button, optional): nút X góc trên

### 1.3. `ItemUI.prefab` (shop item card)
- Component: `ItemUI`
- Cấu trúc:
  - `_imgThumbnail` (Image): ảnh album
  - `_txtName` (TMP_Text)
  - `_txtDescription` (TMP_Text)
  - `_txtPrice` (TMP_Text)
  - `_btnBuy` (Button): nút "Buy"

### 1.4. `OwnedItemUI.prefab` (home item card — không có ảnh)
- Component: `OwnedItemUI`
- Cấu trúc:
  - `_txtName` (TMP_Text)
  - `_txtDescription` (TMP_Text)
  - `_txtPrice` (TMP_Text)
  - `_btnClick` (Button): button phủ toàn card, click show notice

### 1.5. `ScreenShop.prefab` (screen)
- Root GameObject chứa **2 component**:
  - `ScreenShop` (kế thừa `BaseScreen`)
  - `ShopManager`
- Cấu trúc UI:
  - `_btnClose` (Button): nút back về Home
  - `_txtMoney` (TMP_Text): hiển thị tiền hiện tại (VD "$100.00")
  - `_itemContainer` (Transform): GameObject cha chứa items — nên có `GridLayoutGroup` hoặc `VerticalLayoutGroup` + `ContentSizeFitter`
  - Đặt vào ScrollView nếu cần scroll
- Assign vào Inspector:
  - `_itemPrefab` → kéo `ItemUI.prefab` vào
  - `ShopManager._items` có thể để trống (sẽ auto init 5 albums NewJeans + BabyMonster)

### 1.6. `ScreenHome.prefab` (update prefab cũ)
- Root GameObject chứa **2 component**:
  - `ScreenHome` (kế thừa `BaseScreen`)
  - `UserManager`
- Cấu trúc UI:
  - `_btnShop` (Button): nút mở ScreenShop
  - `_itemContainer` (Transform): container chứa items đã mua
- Assign vào Inspector:
  - `_itemPrefab` → kéo `OwnedItemUI.prefab` vào
  - `UserManager._initialMoney` = 100 (hoặc số khác)

---

## 2. UILibrary setup

Mở asset `UILibrary` trong project:
- **Screens**: thêm `ScreenHome.prefab`, `ScreenShop.prefab`
- **Popups**: thêm `PopupNotice.prefab`, `PopupBuyConfirm.prefab`

---

## 3. Scene setup

Scene cần có:
1. **Canvas** (Screen Space — Overlay hoặc Camera)
   - Bên trong tạo 2 GameObject empty:
     - `ScreenLayer` (Transform)
     - `PopupLayer` (Transform, render sau ScreenLayer để đè lên)
2. **UIManager** GameObject (từ `UIManager.prefab` hoặc gắn thủ công)
   - Component `UIManager`
   - Assign: `_screenLayer`, `_popupLayer`, `_library`
3. **GameUI** GameObject
   - Component `GameUI`
   - Khi Start, tự `FindObjectOfType<UIManager>()` và `ShowScreen<ScreenHome>()`

---

## 4. Test flow

1. Play scene → thấy `ScreenHome` + popup welcome
2. Bag rỗng → không có items trên home
3. Click nút Shop → sang `ScreenShop`
4. Thấy list 5 albums + tiền $100.00
5. Click Buy một album → show `PopupBuyConfirm`
6. Confirm → tiền giảm, item vào bag
7. Confirm khi không đủ tiền → show `PopupNotice` "Not enough money"
8. Back về Home → thấy item đã mua
9. Click item ở home → show `PopupNotice` (title = name, content = description)

---

## 5. Tips

- Image URL trong items đang để trống → `_imgThumbnail` sẽ auto disable. Khi test load ảnh, điền URL thật (VD link Wikipedia album cover).
- Nếu popup không hiện: check UILibrary đã register prefab chưa, check `_popupLayer` assigned chưa.
- Nếu items không spawn: check `_itemContainer` và `_itemPrefab` đã assigned chưa.
- Font TMP: nhớ import **TextMesh Pro Essentials** (Window → TextMeshPro → Import TMP Essentials).

---

## 6. Unresolved questions

- Có cần layout đặc biệt cho items không (grid 2 cột, vertical list)? → tuỳ ý thiết kế.
- Popup animation (fade in/out)? → hiện chưa có, override `OnShowAnimation`/`OnCloseAnimation` trong `BasePopup` nếu muốn.
