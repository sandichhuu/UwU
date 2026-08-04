## Modules help developer more easy to implement game features with good structure and fast way on Unity Project.

### Simple MVVM
<img width="912" height="617" alt="image" src="https://github.com/user-attachments/assets/1f29c774-9c0a-41e3-a93d-b9c5932076cd" />

### Fast Bezier Solutions
<img width="1919" height="1031" alt="image" src="https://github.com/user-attachments/assets/f54c7138-01fe-4f52-86e3-480ffc86a97a" />

### A* Path Finding
PriorityCollection using Binary Heap.  
O(1) complexity for fasted get/set element speed.
<img width="1919" height="934" alt="image" src="https://github.com/user-attachments/assets/77adae28-512e-4dc7-afa9-a71bf6720675" />

### FlowField Path Finding
<img width="1919" height="791" alt="image" src="https://github.com/user-attachments/assets/6277dd1e-cec2-4cb0-92ed-9b62c66e287d" />

### Multi-Table DataSet  
* Easy edit on Editor  
* Save to file and load from Bundle (or Addressable).  
* Faster than CSV
* Memory efficient.
<img width="957" height="285" alt="image" src="https://github.com/user-attachments/assets/ffb558cd-b669-420b-8c47-b97625bd2ed3" />  

##### Example Usage  
<img width="818" height="367" alt="image" src="https://github.com/user-attachments/assets/a248f90e-e077-4bf7-bab5-bca02d3bcd0c" />

##### Why this plugin is better than using standard .csv files:

* **Efficient Memory Usage:** With standard `.csv` files, developers typically have to write code that loads the entire file into RAM before parsing, which is slow and memory-intensive.
* **Targeted Data Retrieval:** This plugin loads only the specific records you need. Data is mapped directly across the file structure:
  * For example, if a dataset contains **1 million records** but you only need **4–5 records**, the plugin opens a `FileStream` and seeks directly to the exact byte location required.
* **Easy Schema Definition:** Allows you to define data schemas effortlessly without manual parsing boilerplate.
