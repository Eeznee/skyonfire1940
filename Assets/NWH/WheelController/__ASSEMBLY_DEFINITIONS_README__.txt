
This asset uses Assembly Definition (.asmdef) files. There are many benefits to assembly definitions but a downside is that the whole 
project needs to use them or they should not be used at all.

  * If the project already uses assembly definitions accessing a script that belongs to this asset can be done by adding an reference to the assembly 
  definition of the script that needs to reference the asset. E.g. to access AircraftController adding a NWH.Aerodynamics reference 
  to [MyProjectAssemblyDefinitionName].asmdef is required.

More about Assembly Definitions: https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html