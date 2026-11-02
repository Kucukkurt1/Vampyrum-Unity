from PIL import Image
import os

base = r"c:\Users\PC1\Documents\GitHub\Vampyrum-Unity\Assets\Assets\RF_Catacombs_v1.0"
main = Image.open(os.path.join(base, "mainlevbuild.png"))
dec = Image.open(os.path.join(base, "decorative.png"))
out = r"c:\Users\PC1\Documents\GitHub\Vampyrum-Unity\Assets\_TilePreview"
os.makedirs(out, exist_ok=True)


def save_tile(img, col, row, name, tile=16):
    x, y = col * tile, row * tile
    img.crop((x, y, x + tile, y + tile)).save(os.path.join(out, name + ".png"))


# Composite preview strips for picking tiles
for r in range(28, 40):
    for c in range(44, 64):
        save_tile(main, c, r, f"floor_{c}_{r}")

for r in range(0, 12):
    for c in range(0, 20):
        save_tile(main, c, r, f"wall_{c}_{r}")

for r in range(16):
    for c in range(16):
        save_tile(dec, c, r, f"dec_{c}_{r}")

print("done", len(os.listdir(out)))
