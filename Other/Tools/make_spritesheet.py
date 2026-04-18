"""
Stitches all images in a directory into a single sprite/tile sheet.
Output is always saved to Assets/Textures/Tilesheet/<filename>.

Usage:
    python make_spritesheet.py <input_dir> -o portalEffect.png
    python make_spritesheet.py <input_dir> -o portalEffect.png --cols 4
    python make_spritesheet.py <input_dir> -o portalEffect.png --cols 4 --rows 4
    python make_spritesheet.py <input_dir> -o portalEffect.png --resize 128 128

Frames are sorted alphabetically by filename. If --cols/--rows are omitted,
the grid is auto-calculated to be as square as possible.
"""

import argparse
import math
import sys
from pathlib import Path
from PIL import Image

IMAGE_EXTENSIONS = {".png", ".jpg", ".jpeg", ".bmp", ".tga", ".tiff", ".webp"}


def load_frames(input_dir, resize=None):
    files = sorted(
        f for f in Path(input_dir).iterdir()
        if f.suffix.lower() in IMAGE_EXTENSIONS
    )
    if not files:
        print(f"No images found in {input_dir}")
        sys.exit(1)

    frames = []
    for f in files:
        img = Image.open(f).convert("RGBA")
        if resize:
            img = img.resize(resize, Image.LANCZOS)
        frames.append(img)
        print(f"  Loaded: {f.name} ({img.width}x{img.height})")

    return frames


def build_sheet(frames, cols, rows):
    frame_w = max(f.width for f in frames)
    frame_h = max(f.height for f in frames)

    sheet = Image.new("RGBA", (cols * frame_w, rows * frame_h), (0, 0, 0, 0))

    for i, frame in enumerate(frames):
        col = i % cols
        row = i // cols
        # Center the frame in its cell if it's smaller than the max
        x = col * frame_w + (frame_w - frame.width) // 2
        y = row * frame_h + (frame_h - frame.height) // 2
        sheet.paste(frame, (x, y))

    return sheet, cols, rows, frame_w, frame_h


def main():
    parser = argparse.ArgumentParser(description="Stitch images into a sprite sheet.")
    parser.add_argument("input_dir", help="Directory containing frame images")
    parser.add_argument("-o", "--output", default="spritesheet.png", help="Output filename (saved to Assets/Textures/Tilesheet/)")
    parser.add_argument("--cols", type=int, default=None, help="Number of columns (auto if omitted)")
    parser.add_argument("--rows", type=int, default=None, help="Number of rows (auto if omitted)")
    parser.add_argument("--resize", type=int, nargs=2, metavar=("W", "H"), help="Resize each frame to WxH before stitching")
    args = parser.parse_args()

    print(f"Loading frames from: {args.input_dir}")
    frames = load_frames(args.input_dir, tuple(args.resize) if args.resize else None)
    count = len(frames)
    print(f"  {count} frames loaded\n")

    # Determine grid size
    if args.cols and args.rows:
        cols, rows = args.cols, args.rows
    elif args.cols:
        cols = args.cols
        rows = math.ceil(count / cols)
    elif args.rows:
        rows = args.rows
        cols = math.ceil(count / rows)
    else:
        cols = math.ceil(math.sqrt(count))
        rows = math.ceil(count / cols)

    if cols * rows < count:
        print(f"Error: {cols}x{rows} grid = {cols * rows} cells, but you have {count} frames.")
        sys.exit(1)

    sheet, cols, rows, fw, fh = build_sheet(frames, cols, rows)

    # Always save to Assets/Textures/Tilesheet/
    script_dir = Path(__file__).resolve().parent
    output_dir = script_dir.parent / "Assets" / "Textures" / "Tilesheet"
    output_dir.mkdir(parents=True, exist_ok=True)
    output_path = output_dir / Path(args.output).name

    sheet.save(output_path)
    print(f"Saved: {output_path}")
    print(f"  Grid: {cols} cols x {rows} rows")
    print(f"  Cell size: {fw}x{fh}")
    print(f"  Sheet size: {sheet.width}x{sheet.height}")
    print(f"\n  Use Columns={cols}, Rows={rows} in the FlipbookMaterial settings.")


if __name__ == "__main__":
    main()
