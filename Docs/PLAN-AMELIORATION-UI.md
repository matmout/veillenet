# Plan d'amélioration UI — PC, mobile, animation, bonnes pratiques

Date : 24/11/2026. Suite du commit 727d314 (système de motion).

## 1. Méthode

1. **Mesures live** (navigateur, 375 px et 1280 px, sur les 12 pages qui tournent sans base de données) : débordements horizontaux, cibles tactiles, textes sous 12 px.
2. **Audit de code en 5 lentilles**, parallèles, guidées par les références `impeccable` du dépôt (`.claude/skills/impeccable/reference/`) : responsive des pages, coque et navigation, motion, bonnes pratiques (perf, a11y, hygiène CSS, thèmes), cohérence visuelle.
3. **Vérification adversariale** de chaque constat contre le code : 113 retenus, 2 réfutés. Les correctifs proposés ont été corrigés quand le vérificateur a trouvé mieux (par exemple les couleurs de barres du Radar, qui ne doivent pas dépendre du thème).

Limite : `/`, `/History`, `/AiSummary` et `/NewsletterArchive` répondent 500 en local (PostgreSQL injoignable). Ces pages ont été analysées sur le code seulement.

## 2. Ce que l'audit a trouvé

| Lentille | P0 | P1 | P2 | P3 |
|---|---|---|---|---|
| Responsive (pages) | 2 | 5 | 10 | 7 |
| Coque et navigation | 2 | 6 | 7 | 6 |
| Bonnes pratiques | 2 | 6 | 10 | 7 |
| Cohérence visuelle | 1 | 6 | 9 | 7 |
| Motion | 0 | 3 | 10 | 7 |

P0 = cassé, P1 = expérience nettement dégradée, P2 = finition, P3 = confort.

Les problèmes bloquants :

- **Une accolade en trop** (`}}`, `site.css`) supprimait silencieusement toute la règle `.page-header` : la « fenêtre markdown » des en-têtes n'avait ni cadre ni largeur maximale sur aucune page.
- **Débordement horizontal sur mobile** : des grilles en `minmax(380px, 1fr)` sont plus larges qu'un téléphone (Roadmap, Liens, History, LatestLLM), et la barre de statut sort de l'écran sur toutes les pages.
- **Menu mobile** : chaque groupe reste en `inline-flex`, d'où des colonnes décalées et des libellés coupés ; le panneau fait toute la hauteur, ne défile pas et les derniers liens sont inaccessibles sur un petit écran.
- **Thème clair** : `.text-muted` et `small` sont forcés en `#9ca3af !important` (contraste 2,4:1) ; les alertes et badges gardent des couleurs pastel du thème sombre (1,4:1).

## 3. Décisions transverses

- **Breakpoints** : 768 px reste la seule frontière téléphone/desktop (CSS et JS). Valeurs autorisées en plus : 400, 575.98, 991.98, 1199.98. Les valeurs isolées sont normalisées.
- **Nouveaux tokens** : `--control-h` (36 px, 44 px sur pointeur tactile), `--control-h-sm`, `--chrome-h`, `--content-max` (1440 px), `--measure` (72ch), `--statusbar-bg`, `--statusbar-border`, `--focus-ring`.
- **Composants uniques** : `.briefing-stage` comme colonne de contenu centrée ; `.output-action` comme bouton secondaire partout ; `.filter-bar` + `.filter-pill` comme seul filtre (Radar compris) ; `.chip` pour badges et tags ; `.empty-state` pour les états vide, chargement et erreur ; `.is-pending` sur les boutons d'envoi.
- **Menu mobile** : panneau sous la barre de titre, scrollable, une ou deux colonnes selon la largeur, lignes de 44 px, voile cliquable, fondu de 200 ms à l'ouverture et 120 ms à la fermeture, focus géré, Échap, défilement de la page bloqué.
- **Motion** : toujours « l'IDE ne bouge que quand il se passe quelque chose ». Ajouts : retour au clic (`scale(.97)`), transitions sur les filtres du Radar, retour de réponse dans Training, entrée des messages d'envoi de newsletter, modale IA sans saut. Retraits : effets de survol collants sur tactile, cascade du dashboard rejouée à chaque navigation, boucles infinies décoratives, feu d'artifice opaque qui ne s'arrête jamais.
- **Bonnes pratiques** : styles inline des pages déplacés dans `wwwroot/css/pages/*.css` (mis en cache), `bootstrap.bundle.min.js` chargé seulement sur l'archive, cache immuable pour les fichiers versionnés, ARIA de navigation corrigé (plus de `role="menu"` sur des liens, `aria-current`), focus visible partout, modale IA avec gestion du focus, Knowledge Graph sans injection HTML.

## 4. Lots d'implémentation

Chaque fichier n'a qu'un seul propriétaire. Les lots qui partagent `site.css` passent l'un après l'autre.

| Lot | Périmètre | Fichiers |
|---|---|---|
| 1 — Coque | barre de titre, onglets, menu mobile, barre de statut, skip link, thème, `<head>`, cache statique | `_Layout.cshtml`, régions « chrome » de `site.css`, `theme-toggle.js`, `theme-light.css`, `Program.cs` |
| 1b — Fondations | cadre de page, tokens et composants partagés, thème clair, retour au clic, envois de formulaires | `site.css`, `theme-light.css`, `scrollbar.css`, `site.js`, `postcss.config.js`, partials |
| 2 — Radar | colonne de page, filtres partagés, barres lisibles, transitions de filtre, clavier, séparateur décimal | `Radar.cshtml`, `radar.css`, `radar.js` |
| 3 — Knowledge Graph | plein écran sous le chrome, chargeur qui finit en cas d'erreur, échappement HTML, accessibilité, mouvement réduit | `KnowledgeGraph.cshtml`, `css/pages/knowledge-graph.css`, `js/pages/knowledge-graph.js`, `GraphApiController.cs` |
| P1 — Pages de contenu | grilles, cartes de stats communes, retour dans Training, recherche History, états vides | Index, Roadmap, Training, History, LatestLLM, About, Error, Privacy, AiSummary, Newsletter |
| P2 — Pages catalogue | grilles, chips, logos, filtres de Releases | Liens, MCP, Skills, Releases, `category-cards.css`, `skills.css` |
| M — Modale et newsletter | focus de la modale, mobile, feu d'artifice, confirmation dans la coque, archive | `ai-modal.*`, `fireworks.js`, ConfirmSubscription, ConfirmUnsubscribe, NewsletterArchive |
| 5 — Intégration | changements transmis entre lots, CSS mort, `!important` évitables, alias `--vs-*`, régénération du Bootstrap purgé | tous |

## 5. Hors périmètre (suites possibles)

- Auto-hébergement et sous-ensemble des polices Inter et JetBrains Mono (D14) : demande de télécharger les fichiers et un outil de subsetting. Seul le repli `<noscript>` est fait.
- Sous-ensemble de bootstrap-icons (D13, 91 icônes utilisées sur 2052).
- En-têtes de sécurité et CSP (D19, étape 1 et 3) : la sortie des styles et scripts inline la prépare.
- Balayage complet des tailles et espacements littéraux vers les tokens (E16, E20) : fait seulement dans les fichiers touchés.

## 6. Bilan d'implémentation

Tous les lots sont faits. Les constats non traités sont ceux de la section 5.

**Correctifs ajoutés en vérification** (absents de l'audit) :

- Les résumés de Releases contenaient des URL sans coupure : c'était la vraie cause du débordement à 320 px, que l'audit attribuait à la barre de statut. Corrigé par `overflow-wrap: anywhere` sur les titres et résumés de la timeline.
- Les années de la frise du Radar se recouvraient (étiquettes de 46 px pour un pas de 38 px). Elles sont maintenant compactes et sans cadre.
- Le chargeur du Knowledge Graph affichait « System Loading » sous « Connection failed ». Le sous-titre est masqué en cas d'erreur.

**Issus de la revue de code** :

- L'état « en cours » des boutons d'envoi ne s'applique plus quand la validation côté client annule l'envoi.
- La recherche de History nettoie les résumés côté serveur, comme la première page, et n'accepte que des liens http(s).
- Le bouton Reset de History ne laisse plus de filtre de date invisible.

**Vérifié dans le navigateur** :

- Aucun débordement horizontal à 320 px sur les 13 pages qui tournent sans base. Aucune cible tactile sous 24 px hors liens en texte, aucun texte sous 12 px.
- 768 px : 2 colonnes sur Roadmap et Liens, en-tête aligné sur le contenu.
- 1920 px : colonne de 1440 px centrée, partage 7/5 de Skills.
- Panneau mobile : focus, Échap, contenu inerte, lignes de 44 px.
- Menus desktop au clavier, bouton de thème (icône, libellé, `theme-color`).
- Chargeur du Knowledge Graph en erreur.
- Thèmes clair et sombre.

**Non vérifié en local** (base PostgreSQL injoignable) : Dashboard, History, AI Summary et l'archive de newsletter, la modale IA, les envois de formulaire de newsletter et le Knowledge Graph avec données.

**Chiffres** :

- `bootstrap.purged.min.css` : 52 → 27 Ko.
- `bootstrap.bundle.min.js` (80 Ko) n'est plus chargé que sur l'archive.
- 11 blocs `<style>` inline déplacés dans `wwwroot/css/pages/`.

## Annexe — constats retenus

| Id | Gravité | Lentille | Constat |
|---|---|---|---|
| A1 | P0 | Responsive | Fixed-minimum auto-fill grids overflow the viewport on phones (root cause of /Roadmap and /Liens overflow) |
| A2 | P0 | Responsive | Stray closing brace in site.css silently drops the base .page-header rule on every page |
| B1 | P0 | Coque/nav | Mobile nav panel is as tall as the viewport, is not scrollable, and covers the page |
| B3 | P0 | Coque/nav | Status bar right group overflows the viewport on every page at 375px |
| D1 | P0 | Bonnes pratiques | Light theme: .text-muted and small/.small hard-coded to #9ca3af !important (about 2.4:1 on light surface) |
| D2 | P0 | Bonnes pratiques | Light theme: alerts, Bootstrap badges and the nav NEW badge use dark-mode pastel text (L 0.82-0.85) on near-white |
| E2 | P0 | Cohérence | Grid minmax() literals larger than a phone viewport cause the horizontal overflow on Roadmap, Liens and History |
| A3 | P1 | Responsive | Page header is inset by an extra 12px on mobile and misaligned with the cards below it |
| A5 | P1 | Responsive | Radar page uses a nested container-fluid instead of .briefing-stage, so its header and cards do not align and rhythm differs |
| A6 | P1 | Responsive | No max-width on the content column: header and cards diverge and everything stretches to 1850px on wide desktops |
| A8 | P1 | Responsive | Radar End-of-Support alert items wrap date and 'N days left' badge into 2-3 cramped lines on phones |
| A11 | P1 | Responsive | KnowledgeGraph is not laid out for the viewport: 100vw/100vh canvas inside padded containers, page scrolls under the graph, hardcoded 72px offsets |
| B2 | P1 | Coque/nav | Mobile nav shows two staggered columns because .ide-tab-group stays a row flex container |
| B4 | P1 | Coque/nav | Dark-theme status bar text fails contrast and is 11.2px on mobile |
| B5 | P1 | Coque/nav | Focus ring on status bar links is invisible (same color as the bar) |
| B6 | P1 | Coque/nav | Skip link is hidden behind the sticky header, and anchor jumps land under it |
| B8 | P1 | Coque/nav | NEW badge in the nav is unreadable in the light theme and below 12px |
| B10 | P1 | Coque/nav | Mobile nav toggle lacks Escape, outside-click, focus management and a state-aware label |
| C1 | P1 | Motion | KnowledgeGraph loader never ends on error: infinite spinner overlay blocks the whole site |
| C2 | P1 | Motion | Fireworks cover the confirmation with an opaque black canvas, never stop their rAF loop, and clash with the 'only moves when something real happens' rule |
| C6 | P1 | Motion | Newsletter submit success/error message appears at the bottom of the page with no entrance, scroll or live announcement; no pending state on submit buttons |
| D3 | P1 | Bonnes pratiques | Light theme: Email input placeholder is white at 60% (wins on specificity), invisible on the light CTA form |
| D5 | P1 | Bonnes pratiques | AI summary modal has no focus management (no focus move, trap or return) although it declares aria-modal |
| D6 | P1 | Bonnes pratiques | Nav ARIA misuse (role=menu/menuitem, aria-haspopup on links) and no aria-current or active state on sub-pages |
| D8 | P1 | Bonnes pratiques | Radar bar labels are white 11.5px bold on amber/green gradients (about 1.6-2.3:1) in both themes |
| D9 | P1 | Bonnes pratiques | KnowledgeGraph injects unescaped feed data into innerHTML and into an inline onclick attribute |
| D11 | P1 | Bonnes pratiques | KnowledgeGraph controls are not keyboard/AT accessible, with dark-only low-contrast text and no focus indicator |
| E1 | P1 | Cohérence | .output-action is only styled inside .output-toolbar, so every use outside the dashboard renders as an unstyled native button or link |
| E3 | P1 | Cohérence | Page header (_PageHeaderPartial) does not share the content edge: inset on mobile, capped at 1280px on desktop while cards are not |
| E5 | P1 | Cohérence | Text below the 12px floor: ad-hoc rem literals bypass --text-xs, and the status bar fails contrast |
| E9 | P1 | Cohérence | radar.css is off-system: --vs-* aliases, hex/rgba literals, gradients, hover lift and a 3px side-stripe that the design system forbids |
| E10 | P1 | Cohérence | Light-theme parity is missing for Radar, KnowledgeGraph and several literals; some pairs also fail in dark |
| E22 | P1 | Cohérence | Touch-target minimums are set per component instead of via one token |
| A4 | P2 | Responsive | Double page gutter: Bootstrap container-fluid padding stacks on .briefing-stage padding |
| A7 | P2 | Responsive | Unbounded line length on long-form content (measured 1595px / about 200 characters on 1920px screens) |
| A9 | P2 | Responsive | Radar timeline on touch: no scroll affordance, mouse-only interactions, 4px scrollbar, label column eats 28% of the viewport |
| A10 | P2 | Responsive | Radar timeline positions are emitted with culture-dependent decimals, so bars collapse to left:0 on a non-invariant server culture |
| A12 | P2 | Responsive | KnowledgeGraph floating controls collide on phones; timeline slider is 6px tall and squeezed by a 140px date label |
| A13 | P2 | Responsive | Stale purged Bootstrap bundle: col-xl-7/col-xl-5 missing, so the Skills 7/5 split never happens; dynamic col-md-* classes are invisible to PurgeCSS |
| A14 | P2 | Responsive | Grids with plain 1fr and no min-width:0 let long words, URLs and code push columns wider than the container |
| A15 | P2 | Responsive | Page titlebar overflows its own header at 320px (filename plus meta icon do not fit, no ellipsis) |
| A19 | P2 | Responsive | Newsletter confirmation page is outside the app shell (Bootstrap .container py-5 / p-5 card, no page header) and cramped at 320px |
| A22 | P2 | Responsive | AI summary modal: vh units, 22px close target, no overflow-wrap, desktop dialog proportions on phones |
| B7 | P2 | Coque/nav | Sub-tab menu uses role=menu/menuitem on plain links, and Escape/keyboard handling is incomplete |
| B9 | P2 | Coque/nav | Title bar controls are 28-32px; no coarse-pointer sizing |
| B11 | P2 | Coque/nav | No viewport-fit=cover, safe-area handling, or theme-color meta |
| B12 | P2 | Coque/nav | Theme toggle does not announce state, shows the current theme as the icon, and storage access can throw |
| B13 | P2 | Coque/nav | No aria-current, and the active sub-page is not shown anywhere in the nav |
| B14 | P2 | Coque/nav | Two sub-menus can be open at once, and hover opening is not gated for touch |
| B17 | P2 | Coque/nav | Title bar overflows at 320px |
| C3 | P2 | Motion | 3D knowledge graph: no reduced-motion handling, permanent ambient particle animation, unthrottled graph rebuild on every slider tick |
| C4 | P2 | Motion | No pressed (:active) feedback on any button, pill, tab or icon; --transition-base cannot animate transform |
| C5 | P2 | Motion | Hover transforms and infinite hover animations are not gated by (hover: hover), causing sticky hover on touch |
| C7 | P2 | Motion | Radar filter change: rows teleport, canvas height jumps, cards pop in; re-shown rows replay the full draw-in and pulses |
| C9 | P2 | Motion | Mobile nav opens/closes with no motion and the toggle icon never changes |
| C10 | P2 | Motion | AI summary modal: abrupt loading-to-content swap, unbounded spinner keyframe lives in the modal file, stale responses can overwrite a newer open |
| C11 | P2 | Motion | Training answer feedback has no entrance or state motion and lands below the fold |
| C12 | P2 | Motion | History search: results replaced by 'Loading...' text (layout collapse), hard swap, no busy state, pager does not return to top |
| C13 | P2 | Motion | KnowledgeGraph inline CSS bypasses motion tokens: `transition: all`, hardcoded ease/cubic-bezier, animates `right`, hover translate/scale |
| C14 | P2 | Motion | Unbounded infinite animations: KnowledgeGraph header dot `pulse infinite` and hover-driven `hand-type infinite` |
| D4 | P2 | Bonnes pratiques | Focus ring is invisible on the status bar links (outline color equals the bar background) |
| D7 | P2 | Bonnes pratiques | Radar: timeline bars are mouse-only (no focus, no keyboard, no touch tooltip); filter buttons lack pressed state; Releases misuses aria-pressed on links |
| D10 | P2 | Bonnes pratiques | KnowledgeGraph loads 4 synchronous unpkg scripts and a duplicate Google Fonts stylesheet from inside <main>, without defer or SRI |
| D12 | P2 | Bonnes pratiques | Dark theme contrast: --text-muted on panel-alt/panel-hover is under 4.5:1; status bar text is borderline at 11px |
| D14 | P2 | Bonnes pratiques | Google Fonts via print/onload trick: no <noscript> fallback, FOUT/CLS risk, 3rd-party request, unused weights |
| D15 | P2 | Bonnes pratiques | bootstrap.bundle.min.js (80 KB with Popper) loaded on every page for a single collapse on one page |
| D16 | P2 | Bonnes pratiques | Stylesheet loading: mismatched preload (double fetch), 7 render-blocking CSS files, and inconsistent cache headers |
| D20 | P2 | Bonnes pratiques | Hard-coded dark colors that ignore the light theme: scrollbar.css, radar.css, 6 form/legacy rules, KnowledgeGraph shell |
| D22 | P2 | Bonnes pratiques | Theme robustness: unguarded localStorage, no meta theme-color, light override wins only by source order |
| D23 | P2 | Bonnes pratiques | No forced-colors or prefers-contrast support: active tab, status dot, radar bars and borders depend on background color alone |
| E4 | P2 | Cohérence | Radar page re-implements the page shell (nested container-fluid, no .briefing-stage), so its gutters and rhythm differ from every other page |
| E6 | P2 | Cohérence | Three filter components plus a class-name collision: .filter-btn (Radar), .filter-pill (Releases) and .filter-bar (defined twice) |
| E7 | P2 | Cohérence | Button vocabulary is fragmented: output-action, briefing-cta-submit, tool-link, filter controls and legacy Bootstrap .btn all differ in height, radius and padding |
| E8 | P2 | Cohérence | Badges, tags and chips are seven independent implementations of the same component |
| E11 | P2 | Cohérence | Stat cards are copy-pasted between Roadmap and Training with different class scoping |
| E14 | P2 | Cohérence | ConfirmSubscription page is still raw Bootstrap and skips the IDE page-header pattern |
| E18 | P2 | Cohérence | Vertical rhythm from header to first section doubles up: header margin plus stage gap |
| E21 | P2 | Cohérence | Muted text and primary-button text fall under 4.5:1 on panel-alt and accent fills in the dark theme |
| E23 | P2 | Cohérence | Radar EOL cards: date and 'N days left' badge wrap on mobile because the countdown is not a defined component |
| A16 | P3 | Responsive | Inconsistent breakpoint set (7 different max-width values, 768 inclusive) that disagrees with the Bootstrap bundle and the nav JS |
| A17 | P3 | Responsive | Nested vertical scroll boxes inside the page scroll on touch (scroll traps) |
| A18 | P3 | Responsive | Duplicated component CSS across inline <style> blocks and files (stat-card x2, filter-bar x2, category-card x3, grid pattern x6) |
| A20 | P3 | Responsive | Radar version cards keep two cramped columns of dates on narrow phones |
| A21 | P3 | Responsive | Skills/MCP card grid: dead Bootstrap-column border rules, 6 narrow columns at 1920px, edge borders |
| A23 | P3 | Responsive | About photo stacks full-width with a 280px minimum height on phones |
| A24 | P3 | Responsive | NewsletterArchive mobile layout is fragile and could not be verified (explicit grid-column placement plus order, 600px email in a 250px iframe) |
| B15 | P3 | Coque/nav | Open mobile nav ghosts for the length of the cross-page view transition |
| B16 | P3 | Coque/nav | Status bar semantics: aria-label on spans, a meaningless live region, redundant roles |
| B18 | P3 | Coque/nav | Dead legacy navbar CSS (about 12 !important) still ships |
| B19 | P3 | Coque/nav | Chrome colors and focus rings are hard-coded instead of tokens |
| B20 | P3 | Coque/nav | Sticky chrome plus fixed status bar take 102px on short landscape screens |
| B21 | P3 | Coque/nav | Wrong breakpoint pairs in the chrome: nav at 768, status bar at 720 and 480, Bootstrap at 576/768 |
| C8 | P3 | Motion | Radar version-card hover lift snaps (transform not in transition) and contradicts the panel rule 'no translate' |
| C15 | P3 | Motion | Dashboard panel cascade replays on every navigation on top of the page view-transition (double motion, product register) |
| C16 | P3 | Motion | Bootstrap .collapsing (NewsletterArchive previews) animates height at hardcoded 350ms `ease`, off the motion tokens |
| C18 | P3 | Motion | Theme switch crossfade is muddied by element transitions running underneath |
| C19 | P3 | Motion | Radar tooltip appears/disappears instantly (display none/block) and is positioned with left/top on every mousemove; sticks on touch |
| C20 | P3 | Motion | Permanent will-change and a decorative tilt on the About portrait; stray `filter:none !important` hacks |
| C21 | P3 | Motion | Reduced-motion hygiene: JS reads the preference once at load, `scroll-behavior:smooth` is unconditional, and smooth scroll has no JS twin in site.js |
| D13 | P3 | Bonnes pratiques | Full bootstrap-icons font (130 KB woff2 + 86 KB CSS, 2052 glyphs) for 91 used icons, with font-display:block |
| D17 | P3 | Bonnes pratiques | 53 !important in site.css: about 30 are avoidable (dead navbar block, duplicated Bootstrap overrides) and can be replaced by variable bridging |
| D18 | P3 | Bonnes pratiques | Dead CSS (about 60 classes, roughly 400 lines), 160 legacy --vs-* references, one undefined variable |
| D19 | P3 | Bonnes pratiques | 11 inline <style> blocks, 9 inline <script> blocks, inline on* handlers and style attrs; and no CSP or security headers at all |
| D21 | P3 | Bonnes pratiques | Scrollable regions are not keyboard-focusable (news blocks, briefing output, radar timeline) |
| D24 | P3 | Bonnes pratiques | Images: LCP photo lazy-loaded without dimensions, MCP logos with no size/lazy/decoding, redundant alt text |
| D25 | P3 | Bonnes pratiques | ARIA and semantics noise: redundant roles, aria-live on static text, aria-label on a span, 79 icons without aria-hidden |
| E13 | P3 | Cohérence | Empty, loading and error states have five unrelated treatments and the dashboard's missing-data state is unspecified |
| E15 | P3 | Cohérence | KnowledgeGraph chrome ignores the design tokens, theme and radius scale |
| E16 | P3 | Cohérence | Item-title and numeric font sizes are rem literals that blur the type scale (0.95, 1, 1.05, 1.1, 1.3, 1.4, 1.7rem) |
| E17 | P3 | Cohérence | Section labels (mono uppercase) use three sizes and three letter-spacings |
| E19 | P3 | Cohérence | Inline style blocks and style attributes re-implement shared patterns, and Bootstrap utility hacks sit in shared partials |
| E20 | P3 | Cohérence | Radius, shadow and spacing literals bypass the tokens (radii 2-16px, rgba shadows, ~125 rem spacing values) |
| E24 | P3 | Cohérence | Dead and legacy aliases remain in the shared stylesheet and encourage drift |
