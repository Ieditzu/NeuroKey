import os
import glob

# Walk through all relevant files
for root, dirs, files in os.walk('NeuroKey'):
    for file in files:
        if file.endswith(('.cs', '.unity', '.prefab', '.asset')):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
            if 'SphereController' in content:
                content = content.replace('SphereController', 'BeanController')
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(content)

# Rename the script and its meta file
old_script = 'NeuroKey/Assets/Scripts/Runtime/SphereController.cs'
new_script = 'NeuroKey/Assets/Scripts/Runtime/BeanController.cs'
if os.path.exists(old_script):
    os.rename(old_script, new_script)
if os.path.exists(old_script + '.meta'):
    os.rename(old_script + '.meta', new_script + '.meta')

print("Renaming complete.")
