# HealthVault Images

## Recommended Images for Authentication Pages

### Login Page
**Theme**: Medical professional, healthcare provider, or doctor
**Suggested images**:
- Doctor with stethoscope
- Medical professional with tablet
- Healthcare team
- Modern hospital interior
- Friendly nurse or doctor portrait

**Dimensions**: 800x1000px (portrait) or 1200x800px (landscape)
**Format**: JPG or PNG
**File name**: `login-hero.jpg` or `login-hero.png`

### Register Page
**Theme**: Hospital building, healthcare facility, or welcoming medical environment
**Suggested images**:
- Modern hospital exterior
- Clinic reception area
- Medical facility entrance
- Healthcare team welcoming patients
- Bright, clean medical environment

**Dimensions**: 800x1000px (portrait) or 1200x800px (landscape)
**Format**: JPG or PNG
**File name**: `register-hero.jpg` or `register-hero.png`

## Free Stock Photo Resources

### High-Quality Medical Images:
1. **Unsplash** - https://unsplash.com/s/photos/doctor
2. **Pexels** - https://www.pexels.com/search/healthcare/
3. **Pixabay** - https://pixabay.com/images/search/hospital/
4. **Freepik** - https://www.freepik.com/ (some free, some premium)

### Search Terms:
- "medical professional"
- "doctor portrait"
- "hospital modern"
- "healthcare team"
- "clinic interior"
- "medical technology"
- "stethoscope"

## How to Replace SVG with Real Images

### In Login.razor:
Replace this line:
```html
<svg class="auth-image-illustration" ...>
```

With:
```html
<img src="/images/login-hero.jpg" class="auth-image-illustration" alt="Medical Professional" />
```

### In Register.razor:
Replace this line:
```html
<svg class="auth-image-illustration" ...>
```

With:
```html
<img src="/images/register-hero.jpg" class="auth-image-illustration" alt="Hospital" />
```

## Image Optimization Tips

1. **Compress images**: Use tools like TinyPNG or ImageOptim
2. **Target size**: Keep under 500KB for web performance
3. **Format**: Use WebP for best compression (with JPG fallback)
4. **Responsive**: Images adapt automatically to screen size

## Current Status

Currently using **SVG illustrations** as placeholders. These are:
- ✅ Lightweight (no extra file downloads)
- ✅ Scalable (perfect on any screen)
- ✅ Customizable (colors match your theme)
- ✅ Animated (subtle pulse effects)

Replace with real photos when you have them!
