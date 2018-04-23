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