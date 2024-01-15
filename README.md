# MassConvertFromModel
Mass converts the following FromSoftware model types to FBX:  
FLVER0  
FLVER2  
MDL4  
SMD4  

SMD4 is not fully explored and may not convert right.  

Uses AssimpNet which has limitations regarding FLVER currently.  
FBX exported by AssimpNet seems to require the import scale to be set to 100.0 for a properly scaled import in Blender.  

Will be slow when mass converting many files.  

# Usage
1. Drag and drop a folder or file onto the exe  
2. Import FBX in Blender with scale setting set to 100.0  

# Building
1. [Download SoulsFormatsExtended][0]  
1. [Download FromAssimp][1]  
2. Place SoulsFormatsExtended's repo and FromAssimp's repo into the folder containing this repo's folder.  
3. Build this repo in VS 2022.

[0]: https://www.github.com/WarpZephyr/SoulsFormatsExtended
[1]: https://www.github.com/WarpZephyr/FromAssimp