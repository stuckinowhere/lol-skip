# WASD brand kit

House mark for Unqueued, wasdlol, and other apps in this family.

Not Riot art. Hex + WASD keys, colored from [palette 8528](https://www.color-hex.com/color-palette/8528).

## Palette

| Token  | Hex       | Use                          |
|--------|-----------|------------------------------|
| mint   | `#0AC8B9` | borders, highlight, the mark |
| teal   | `#0397AB` | secondary labels             |
| deep   | `#005A82` | hex fill, button press       |
| panel  | `#0A323C` | cards, inner chrome          |
| ink    | `#091428` | window background            |
| text   | `#E8FBFA` | body copy (not in the 5)     |

## Files to copy

- `wasd-mark.svg` — source
- `wasd-mark.png` / `wasd-mark.ico` — app and tray
- `../Controls/WasdMark.cs` — Avalonia control (hex + keys, compact W under 26px)
- `colors.json` — tokens

In another app, set the window background to ink, strokes to mint, and drop `<WasdMark Width="22" Height="22"/>` in the chrome.
