import re

filepath = 'NeuroKey/Assets/Scripts/Editor/GameSceneBuilderEditor.cs'
with open(filepath, 'r') as f:
    content = f.read()

# Let's see what the original logic was for Medium and Hard challenges. They both seem empty! 
# Let's see if there was some logic in the git history before I accidentally deleted it in my "bloat" pass, 
# or if it was removed in my script.
