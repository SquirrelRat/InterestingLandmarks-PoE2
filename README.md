# InterestingLandmarks for PoE2

A comprehensive landmark visualization utility for **ExileCore2**, highlighting points of interest on the minimap to aid exploration and looting.

---

<img width="147" height="124" alt="image" src="https://github.com/user-attachments/assets/a6b0d802-fcd9-4640-9c82-4598b7323e81" />
<img width="168" height="91" alt="image" src="https://github.com/user-attachments/assets/c6e41d16-1d5f-41aa-bd8d-9cddfd0dd97a" />
<img width="207" height="76" alt="image" src="https://github.com/user-attachments/assets/d94fa0b2-9184-4d2e-b05e-949cf8e4bc03" />


## Features

*   **Chests**: Display chests by rarity (White, Magic, Rare, Unique) with distance-based alpha fading for White chests.
*   **Strongboxes**: Specialized support for Arcanist's, Cartographer's, Diviner's, Unique, and other strongboxes with distinct colors.
*   **Area Transitions**: Show entrances/exits to other areas.
*   **Waypoints**: Highlight waypoint locations.
*   **Points of Interest**: Display various map markers like expeditions (excluding rituals).
*   **Essences**: Show essence monoliths with optional dynamic labels revealing essence types.
*   **Shrines**: Mark shrines with customizable labels.
*   **Breaches**: Indicate breach locations.
*   **Rituals**: Display ritual runes map-wide, unaffected by distance limits.
*   **Switches**: Highlight interactive switches.
*   **Clustering**: Group nearby landmarks of the same type into a single label with count (e.g., "Chest (x5)").
*   **Performance**: Efficient caching with adjustable update intervals and render distances.

---

## Installation

1.  Place the `InterestingLandmarks-PoE2` folder into `ExileCore2/Plugins/Source/`.
2.  Restart **ExileCore2**.
3.  Enable **InterestingLandmarks** in the settings menu (**F12**).

---

## Configuration

*   **Master Enable**: Toggle the entire plugin on/off.
*   **Master Toggle Hotkey**: Quick enable/disable keybind.
*   **Max Render Distance**: Distance from player beyond which landmarks are hidden (except Rituals).
*   **Update Interval**: How often to scan for landmarks (lower = more responsive, higher CPU).
*   **Enable Dynamic Labels**: Show detailed labels (e.g., essence types, shrine names).
*   **Enable Clustering**: Group nearby same-type landmarks; adjust cluster radius.
*   **Chests/Strongboxes**: Toggle and color-code by rarity/type.
*   **Area Transitions/Waypoints/PoI**: Enable and set colors.
*   **Essences/Shrines/Breaches/Rituals/Switches**: Toggle and color settings.
