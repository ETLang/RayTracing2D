RT2D Material Characteristics
----------------------------
Mask (Map)
Color (including opacity)   (Mappable)
Emission (Mappable, Brightness, V2)
Outscattered Emission (Mappable, Brightness)
Dielectric (Mappable)
Surface Smoothness (Mappable)
Refraction Index

Scattering Strength (Mappable, Ranged)

RT2D Sprite Characteristics
-------------------------
Material
Mask
Surface Normal (Mappable?? Where does this come from and how is it represented?)
Interior Normal (Mappable, Quantum Interactivity)

RT2D Atmosphere Characteristics
------------------------------
Scattering Strength
Rayleigh Scattering Wavelength
Rayleigh Scattering Strength



RT2D world construction
-----------------------

The 2D world consists of layers.
Each layer has a collection of sprites, an atmosphere, and photonic depth

A sprite is a 2D shape rendered using an RT2D material.
Sprites are rendered bottom-up? Or top-down?