import os

filepath = 'NeuroKey/Assets/Scripts/Editor/TopDownSceneBuilderEditor.cs'
with open(filepath, 'r') as f:
    lines = f.readlines()

out_lines = []
skip = False
for line in lines:
    if 'var travelController = root.GetComponent<SkinTubeTravelController>();' in line:
        skip = True
    if 'travelController.Configure(ejectTube, ejectTarget);' in line:
        skip = False
        continue
    if not skip:
        out_lines.append(line)

with open(filepath, 'w') as f:
    f.writelines(out_lines)
