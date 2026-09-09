# WASD brand kit

House mark for wasdlol skip and other apps in this family.

Not Riot art. Hex + WASD keys, colored from the gold-on-ink palette.

## Palette

| Token | Hex       | Use                          |
|-------|-----------|------------------------------|
| ink   | `#010A13` | window background, hex fill  |
| bone  | `#F0E6D2` | body copy, number highlight  |
| gold  | `#C8AA6E` | borders, labels, the mark    |
| panel | `#070B12` | inner chrome                 |
| key   | `#16110A` | WASD key fill                |

## Files to copy

- `wasd-mark.svg` — source
- `wasd-mark.png` / `wasd-mark.ico` — app and tray
- `../Controls/WasdMark.cs` — Avalonia control (hex + keys, compact W under 26px)
- `colors.json` — tokens

In another app, set the window background to ink, strokes to gold, and drop `<WasdMark Width="22" Height="22"/>` in the chrome.
