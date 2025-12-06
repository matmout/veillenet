# Performance Optimization - WebP Image Conversion

## Converting PNG to WebP

Pour optimiser les images du site, convertir les PNG en WebP:

### Méthode 1: Outil en ligne (rapide)
- https://cloudconvert.com/png-to-webp
- https://squoosh.app/ (recommandé - Google)

### Méthode 2: CLI (pour automatisation)
```bash
# Installer cwebp (Google WebP tools)
# Windows: choco install webp
# macOS: brew install webp
# Linux: apt-get install webp

# Convertir une image
cwebp -q 85 containsharp.png -o containsharp.webp

# Batch conversion
for file in *.png; do cwebp -q 85 "$file" -o "${file%.png}.webp"; done
```

### Méthode 3: C# Script (intégration CI/CD)
```csharp
// Utiliser ImageSharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

var encoder = new WebpEncoder { Quality = 85 };
using var image = Image.Load("containsharp.png");
image.Save("containsharp.webp", encoder);
```

## Images actuelles à convertir

wwwroot/icons/containsharp.png ? wwwroot/icons/containsharp.webp

## Usage dans HTML avec fallback

```html
<picture>
  <source srcset="~/icons/containsharp.webp" type="image/webp">
  <img src="~/icons/containsharp.png" alt="ContainSharp Logo" width="64" height="64" loading="lazy">
</picture>
```

## Bénéfices attendus
- Réduction taille: ~30-50% vs PNG
- Meilleur Core Web Vitals (LCP)
- Moins de bande passante
