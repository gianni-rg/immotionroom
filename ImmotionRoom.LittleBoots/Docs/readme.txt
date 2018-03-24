This project contains post-compilation scripts that copy the produced DLLs inside all Unity projects using it inside this suite.
Notice that this means including also all PDB files, that have to be REMOVED in the distribution releases, for security reasons.

There is a known incompatibility issue with VS2015/VS2017 and the PDB2MDB Tool included in Unity.
We need to use an updated PDB2MDB Tool available here: https://gist.github.com/jbevain/ba23149da8369e4a966f#file-pdb2mdb-exe
A copy of the updated tool has been placed in the Unity3d\Tools folder and the post-compilation script reference this tool.
The tool is compatible with both VS2013, VS2015 and VS2017.

# OLDER VERSION -- IF YOU WANT TO USE THE PDB2MDB Tool included in Unity #
To launch post-compilation events, you have to set the environment variable UNITYMONOPATH to the Unity Mono tools path.
Usually, this path is: C:\Program Files\Unity\Editor\Data\Mono\lib\mono\2.0

In project Avateering, pdb2mdb crashes because of its own bugs. Don't worry. This only means you can't debug that DLL from Unity Editor

Custom UMA-like avatars, must have Root node with local scale of 1,1,1. Otherwise it is set to this value to fix a UMA behaviour

UMA has known issues... check the roadmap on Asana

UMA Unity plugin... to play with it you must copy all the files of Skeletal DLL into ImmotionRoom/Plugin, or the plugin won't work
(at the moment they are present in a folder called CopyBefore...)