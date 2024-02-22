# MassConvertFromModel
Mass converts the following FromSoftware model types to AssimpNet to be exported:  
FLVER0  
FLVER2  
MDL4  
SMD4  

SMD4 is not fully explored and may not convert right.  

Will be slow when mass converting many files.  

# Usage
1. Drag and drop a folder or file onto the exe  
2. If using FBX, Import FBX into Blender with scale setting set to 100.0  

# Issues
If the program closes immediately upon opening it, make sure you have the needed .NET runtime installed.  
This project is currently using .net8.0 in its latest versions, and older versions may be using .net6.0.  
This can be confirmed as the issue by running the program from the command line and seeing if it says .NET is missing.  

If any other errors come up, please let me know by opening an issue.  

# Known Export Issues
This program uses AssimpNet which has limitations regarding FLVER currently.  
FBX exported by AssimpNet seems to require the import scale to be set to 100.0 for a properly scaled import in Blender.  

I cannot convert normals correctly for some reason, but these can be cleared under custom normal data in Blender.  
UV maps have similar problems, but are correctable according to some people who have used my tool.  

Some models in later PS3 games using FLVER2 have edge compressed vertex buffers.  
I managed to solve decompressing edge compressed index buffers, but not vertex buffers completely yet.  
The models I saw using edge compressed vertex buffers also seem to have uncompressed copies of them for some reason.  
Currently I'm ignoring vertex buffers that are edge compressed as I cannot read them all correctly.  
I have an idea of how they work, if anybody is interested in figuring the rest out, let me know.  

Xbox 360 textures from TPF files are swizzled in a way that is not known quite yet.  
They cannot be properly read.  

I believe PS4 TPFs can be read, but I don't think my fork of SoulsFormats currently supports them.  
I'll look into that in the future as apparently DSMapStudio does.  

# Building
1. [Download SoulsFormatsExtended][0]  
1. [Download FromAssimp][1]  
2. Place SoulsFormatsExtended's repo and FromAssimp's repo into the folder containing this repo's folder.  
3. Build this repo in VS 2022.

[0]: https://www.github.com/WarpZephyr/SoulsFormatsExtended
[1]: https://www.github.com/WarpZephyr/FromAssimp