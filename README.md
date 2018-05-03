# DC-IO

## Introduction

Provides various IO/Persistence providers for use within projects. Using a simple Set/Get/Remove mechanism serialised data can be stored against known keys in the chosen storage mechanism. The following storage mechanisms are implemented:
- Azure Cosmos
- Azure Storage
- Azure Table Storage
- Dictionary (In Process)
- File System
- Redis
- SQL Server

## Usage

All providers implement the IKeyValuePersistenceService interface from the interface package. All constructors have a dependency on the logging library, and optionally a configuration interface. The configuration interface expects the connectivity details for the respective underlying persistence provider. This information can usually be found within the Azure portal.

The implementations have been optimised to provide high throughput; a performance harness project is included in this solution for comparison and further optimisation purposes.

##### Azure Cosmos

Provides an integration with Azure Cosmos, Microsoft's globally distributed, multi-model database. Data is stored as documents. Caching is used internally on the URLs (not the data), and the endpoint URL has the connection limit expanded to 1000 concurrent connections.

##### Azure Storage

Provides an integration with Azure Storage, storing data in files. The key is used as the filename, with the content stored in the file. The endpoint URL has the connection limit expanded to 1000 concurrent connections.

##### Azure Table Storage

Provides an integration with Azure Table Storage, storing data in a schema consisting of key/value pairs. The endpoint URL has the connection limit expanded to 1000 concurrent connections.

##### Dictionary

Provides in-process key/value pair storage using a standard concurrent dictionary.

##### File System

Provided computer/VM wide storage of key/value pairs using files on the filesystem. The key is used as the filename, with the content stored in the file.

##### Redis

Provides an integration with (Azure) Redis. Caching is used internally on the URLs (not the data), and the endpoint URL has the connection limit expanded to 1000 concurrent connections.

##### SQL Server

Provides an integration with SQL Server. Supports both normal and in-memory tables. Database project is included. The key should be formatted as JobId_Item_ActorId e.g. 1089_2_4.

## Results

### Single Threaded

```
sRuns: 250; String Length: 10000; Multi: False; Please wait...
Azure Storage - Sum: 8716 [4113,2120,2483]; Average: 34.864 [16.452,8.48,9.932]
Azure Table - Sum: 6905 [0,2338,4567]; Average: 27.62 [0,9.352,18.268]
Azure Cosmos - Sum: 5481 [2642,1477,1362]; Average: 21.924 [10.568,5.908,5.448]
File System - Sum: 3091 [3080,7,4]; Average: 12.364 [12.32,0.028,0.016]
SQL Server - Sum: 2355 [1327,421,607]; Average: 9.42 [5.308,1.684,2.428]
Redis - Sum: 1942 [938,646,358]; Average: 7.768 [3.752,2.584,1.432]
Dictionary - Sum: 1 [1,0,0]; Average: 0.004 [0.004,0,0]
```
```
sRuns: 250; String Length: 1000; Multi: False; Please wait...
Azure Storage - Sum: 24989 [19917,2376,2696]; Average: 99.956 [79.668,9.504,10.784]
Azure Table - Sum: 6219 [0,2147,4072]; Average: 24.876 [0,8.588,16.288]
Azure Cosmos - Sum: 5376 [2583,1456,1337]; Average: 21.504 [10.332,5.824,5.348]
SQL Server - Sum: 1808 [1025,314,469]; Average: 7.232 [4.1,1.256,1.876]
Redis - Sum: 917 [472,215,230]; Average: 3.668 [1.888,0.86,0.92]
File System - Sum: 655 [646,5,4]; Average: 2.62 [2.584,0.02,0.016]
Dictionary - Sum: 0 [0,0,0]; Average: 0 [0,0,0]
```
```
sRuns: 250; String Length: 100; Multi: False; Please wait...
Azure Storage - Sum: 22811 [18533,1869,2409]; Average: 91.244 [74.132,7.476,9.636]
Azure Table - Sum: 6877 [0,2154,4723]; Average: 27.508 [0,8.616,18.892]
Azure Cosmos - Sum: 5407 [2594,1447,1366]; Average: 21.628 [10.376,5.788,5.464]
SQL Server - Sum: 1964 [1190,272,502]; Average: 7.856 [4.76,1.088,2.008]
Redis - Sum: 1372 [657,349,366]; Average: 5.488 [2.628,1.396,1.464]
File System - Sum: 651 [635,3,13]; Average: 2.604 [2.54,0.012,0.052]
Dictionary - Sum: 0 [0,0,0]; Average: 0 [0,0,0]
```

### Multi Threaded

```
mRuns: 250; String Length: 100; Multi: True; Please wait...
Azure Storage - Sum: 53663 [36763,7443,9457]; Average: 3833.07142857143 [2625.92857142857,531.642857142857,675.5]
SQL Server - Sum: 23799 [15061,3027,5711]; Average: 1699.92857142857 [1075.78571428571,216.214285714286,407.928571428571]
Redis - Sum: 16550 [7157,5911,3482]; Average: 1182.14285714286 [511.214285714286,422.214285714286,248.714285714286]
Azure Table - Sum: 15310 [0,5367,9943]; Average: 1093.57142857143 [0,383.357142857143,710.214285714286]
Azure Cosmos - Sum: 2793 [2712,70,11]; Average: 2793 [2712,70,11]
File System - Sum: 273 [272,1,0]; Average: 19.5 [19.4285714285714,0.0714285714285714,0]
Dictionary - Sum: 0 [0,0,0]; Average: 0 [0,0,0]
```
```
mRuns: 250; String Length: 1000; Multi: True; Please wait...
Azure Storage - Sum: 44623 [32481,7276,4866]; Average: 3718.58333333333 [2706.75,606.333333333333,405.5]
Azure Table - Sum: 16991 [0,6718,10273]; Average: 1415.91666666667 [0,559.833333333333,856.083333333333]
SQL Server - Sum: 9453 [6682,440,2331]; Average: 787.75 [556.833333333333,36.6666666666667,194.25]
Redis - Sum: 7472 [5024,1576,872]; Average: 622.666666666667 [418.666666666667,131.333333333333,72.6666666666667]
Azure Cosmos - Sum: 2588 [2502,74,12]; Average: 2588 [2502,74,12]
File System - Sum: 117 [96,15,6]; Average: 9.75 [8,1.25,0.5]
Dictionary - Sum: 2 [1,1,0]; Average: 0.166666666666667 [0.0833333333333333,0.0833333333333333,0]
```
```
mRuns: 250; String Length: 10000; Multi: True; Please wait...
Azure Storage - Sum: 52608 [36774,6803,9031]; Average: 3757.71428571429 [2626.71428571429,485.928571428571,645.071428571429]
SQL Server - Sum: 25839 [16657,1504,7678]; Average: 1845.64285714286 [1189.78571428571,107.428571428571,548.428571428571]
Redis - Sum: 17448 [6332,3652,7464]; Average: 1246.28571428571 [452.285714285714,260.857142857143,533.142857142857]
Azure Table - Sum: 14357 [0,3568,10789]; Average: 1025.5 [0,254.857142857143,770.642857142857]
Azure Cosmos - Sum: 2988 [2875,96,17]; Average: 2988 [2875,96,17]
File System - Sum: 226 [217,6,3]; Average: 16.1428571428571 [15.5,0.428571428571429,0.214285714285714]
Dictionary - Sum: 0 [0,0,0]; Average: 0 [0,0,0]
```