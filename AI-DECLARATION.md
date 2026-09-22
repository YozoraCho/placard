---
version: "0.1.2"
level: pair
processes:
  design: assist
  implementation: pair
  testing: none
  documentation: pair
  review: assist
components:
  Placard/Core/Housing/: assist
  Placard/Core/Localization/: assist
  Placard/Localization/: assist
  Placard/Windows/Housing/: assist
  Placard/Windows/Components/: pair
---

This format is based on [AI-DECLARATION.md](https://ai-declaration.md/en/0.1.2/).

## Notes

Placard was built with Claude (Anthropic) as a development assistant. This file is an honest account of where AI was involved and where it was not.

**Overall level, `pair`:** the bulk of this repository is the author's own code, extracted from the Housing app in [Aetherphone](https://github.com/XeldarAlz/FFXIV-Aetherphone). AI worked on that code alongside the author rather than in place of them. The housing layer, map rendering, plot markers, filters and the pickers are the author's; AI carried out the extraction, rebuilt the window for a desktop layout, and added the world-wide plot list. Architecture was argued out between the two, and the author understands how every part of it works.

### By area

**`Placard/Core/Housing/` (`assist`)** — the author's work. The data model, housing API client, freshness handling, filters, watchlist, reminders and travel integration were designed and written before Placard existed. AI performed the extraction and reworked the refresh path so a single request populates every district instead of one.

**`Placard/Windows/Housing/` (`assist`)** — the author's work in the main. Map rendering, plot markers, the ward plan, the filter drawer, the world picker, plot details, the watchlist and the settings screens all carry over. AI ported them, rebuilt the layout for a resizable desktop window in place of the phone frame, added the world-wide plot list and the map and list view switch, restyled the three selectors onto shared tokens, and fixed the visual defects the author reported.

**`Placard/Windows/Components/` (`pair`)** — mixed. The drawing and interaction primitives are the author's: geometry, hit testing, spacing tokens, text styles, chip rails, grouped cards and toggles. AI wrote the pieces the standalone window needed that the phone did not, including the window chrome, the confirm prompt, the selection tokens shared by the three selectors, and icon rendering, and rewrote the typography layer to sit on Dalamud's font stack rather than bundled fonts.

**Localization (`assist`)** — the nine catalogs came from the Aetherphone project. Five keys added for the standalone build and three reworded during the extraction are machine-translated and have not been reviewed by native speakers. They are flagged as placeholders in the README and corrections are welcome.

**Design (`pair`)** — architecture was discussed rather than dictated in either direction. AI proposed structures and argued for them; the author accepted some, rejected others, and settled the product questions, including making the world-wide list a peer of the map rather than a detour off it.

**Testing (`none`)** — there are no automated tests in this repository. Every build was compiled and exercised in game by the author.

**Review (`assist`)** — the author reviewed all generated code. AI performed one pre-submission sweep for stale references and dead code.

### What AI did not do

- Decide what Placard should be, or how it should behave.
- Test the plugin. Every in-game verification was done by a human.
- Design the housing data model, the API contract, or the caching service behind it.
- Write the map rendering, the plot markers, the filter model, or the world picker. Those came from the author's earlier work.
- Produce any asset. There are no AI-generated images, icons, or sounds in this repository.

### Stance

AI involvement is not an excuse for anything being wrong. If you find a bug, a mistranslation, or something that looks like it was written without understanding, open an issue and it will be treated as the author's responsibility, because it is.