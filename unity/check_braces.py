filepath = 'NeuroKey/Assets/Scripts/Editor/TopDownSceneBuilderEditor.cs'
with open(filepath, 'r') as f:
    text = f.read()

count = 0
for i, char in enumerate(text):
    if char == '{':
        count += 1
    elif char == '}':
        count -= 1
    if count < 0:
        print(f"Error: unmatched closing brace at index {i}")
        break

if count != 0:
    print(f"Error: unbalanced braces, net count: {count}")
else:
    print("Braces are balanced!")
