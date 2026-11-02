from PIL import Image
import os

base = r"c:\Users\PC1\Documents\GitHub\Vampyrum-Unity\Assets\Assets\RF_Catacombs_v1.0"
main = Image.open(os.path.join(base, "mainlevbuild.png")).convert("RGBA")
w, h = main.size
tile = 16
cols, rows = w // tile, h // tile

non_empty = []
for row in range(rows):
    for col in range(cols):
        x, y = col * tile, row * tile
        region = main.crop((x, y, x + tile, y + tile))
        pixels = list(region.getdata())
        alpha_sum = sum(p[3] for p in pixels)
        if alpha_sum > 500:
            non_empty.append((col, row, alpha_sum))

print(f"grid {cols}x{rows}, non-empty tiles: {len(non_empty)}")

# Save overview with grid labels for first content region
out = r"c:\Users\PC1\Documents\GitHub\Vampyrum-Unity\Assets\_TilePreview"
os.makedirs(out, exist_ok=True)

# Group by row for readability
from collections import defaultdict
by_row = defaultdict(list)
for c, r, a in non_empty:
    by_row[r].append(c)

for r in sorted(by_row.keys()):
    cols_in_row = sorted(by_row[r])
    print(f"row {r:2d}: cols {cols_in_row[0]}-{cols_in_row[-1]} ({len(cols_in_row)} tiles)")

# Extract sample floor candidates (rows 30+)
print("\nFloor candidates (row>=30):")
for c, r, a in non_empty:
    if r >= 30:
        print(f"  ({c},{r}) alpha={a}")

# Extract wall candidates (rows 0-15, cols 0-30)
print("\nWall candidates (row<=15, col<=30):")
for c, r, a in non_empty:
    if r <= 15 and c <= 30:
        print(f"  ({c},{r}) alpha={a}")

# Save a few promising tiles
samples = [(52, 33), (53, 33), (54, 33), (55, 33), (56, 33), (57, 33),
           (52, 34), (53, 34), (54, 34), (55, 34), (56, 34), (57, 34),
           (2, 2), (3, 2), (4, 2), (5, 2), (6, 2),
           (2, 3), (3, 3), (4, 3), (5, 3), (6, 3),
           (10, 0), (11, 0), (12, 0), (13, 0)]
for c, r in samples:
    x, y = c * tile, r * tile
    main.crop((x, y, x + tile, y + tile)).save(os.path.join(out, f"sample_{c}_{r}.png"))
