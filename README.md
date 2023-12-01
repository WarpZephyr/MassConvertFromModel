# MassConvertFromModel
Mass converts the following FromSoftware model types to FBX:  
FLVER0  
FLVER2  
MDL4  
SMD4  

Not all models are supported, only ones that weight to a single bone, other models will not convert right.  
SMD4 is not fully explored and may not convert right.  

Uses AssimpNet which has limitations regarding FLVER currently.  
FBX exported by AssimpNet seems to require the import scale to be set to 100.0 for a properly scaled import in Blender.  

Will be slow when mass converting many files.  

# Usage
1. Drag and drop a folder or file onto the exe  
2. Import FBX in Blender with scale setting set to 100.0