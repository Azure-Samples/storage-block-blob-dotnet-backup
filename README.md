# Sample Backup Solution for Azure Block Blobs

Today, Azure Blob storage doesn't offer an out of box solution for backing up block blobs. This sample can be used to perform daily incremental back-ups of storage accounts containing block blobs. 

In case of a disaster, the solution also provides an option to restore the storage account. 

## Features
This project framework provides the following features:
* Daily Incremental backup of storage account
* Restore backup by providing start date and end date 

## Prerequisites
* Visual Studio 2017
* Docker for Windows

## Instructions to Set-up Solution
### Set up Event Grid for Source Storage account.
* Create an Azure storage queue first to store the event grid events. Please note this storage account containing the storage queue cannot be connected to VNET.
* Create an event subscription for the storage account by firing the below command in the Azure Command Shell. Please note, this feature is still in preview and you will have to install the Event Grid extension for Azure CLI. You can install it with az extension add --name eventgrid. This step will make sure all the create, replace and delete events are recorded in the configured Azure Storage Queue. **Please note modifications to a BLOB are not recorded hence our solution won’t be able to support the same.**

```
az eventgrid event-subscription create \
--resource-id "/subscriptions/<<subscriptionid>>/resourceGroups/<<resourcegroupname>>/providers/Microsoft.Storage/storageAccounts/<<sourcestorage>>" \
-g "<<resourcegroupname>>" \
--name "<<subscriptionname>>" \
--endpoint-type storagequeue \
--endpoint "/subscriptions/<<subscriptionid>>/resourcegroups/<<resourcegroupname>>/providers/Microsoft.Storage/storageAccounts/<<storageaccountname>>/queueservices/default/queues/<<queuename>>"
```

### Set up .Net Core Projects to perform incremental backup and restore backup.

The sample solution has three projects in it.

* ##### backup.core	
This project does not require any modification. It contains the logic of backup, and restore.
* ##### backup.utility	
This contains the main endpoint to start the backup process. This is a continuous running utility and performs the incremental backup every X second. It also creates a detailed log for you to monitor the backup process. It uses Serilog and stores the logs in Azure Table Storage. You can change it as needed. Here are the settings which are configurable. Highlighted settings are mandatory to change rest can can be left with default values

**Connection Strings**

| Key Name        | Description           |
| ------------- |:-------------| 
| **EventQueueStorage**      | This the connection string to storage account where the Azure Storage queue exists and where the azure storage events are being stored. | 
| **SourceBlobStorage**      | This is the connection string of the source storage account for which the back-up needs to be performed. | 
| **BackupTableStorage**      | This is the connection string of the storage account where the storage events for the incremental backup will be stored. The listener will read the event messages from the storage queue and will store the same here with some additional info. |
| **BackupBlobStorage**      | This is the connection string of the destination storage account where the created/replaced/deleted block blobs are copied. |	

**App Settings**

| Key Name        | Description           |
| ------------- |:-------------| 
| **BackupTableName**      | Create a storage table manually in storage account mentioned in the “BackupTableStorage” connection string and specify the name of the same table here. | 
| **EventQueueName**      | Specify the Name of the event queue created above in “EventQueueStorage” connection string. | 
| QueueVisibilityTimeOutInMS      | Queue visibility time out in milliseconds. The listener must process the messages in this time frame before it starts appearing in the queue again. In case, the messages are processed successfully they won’t appear in storage queue again. | 
| QueueMessageCountToRead      | Defines the number of messages to read in one batch by the listener from the Azure Storage Queue. Please note utility reads another batch of messages only when it’s done processing the existing batch. | 
| BlobSASExpiryInMts      | SAS Key Expiry in Minutes. This is used in case of Server copy.| 
| TimerElapsedInMS      | Timer interval in milliseconds. On this timer click the listener reads the messages from event queue and copies the events metadata to table storage and copies the blobs from source storage account to destination storage account. |
|IsServerCopy|	To perform server copy or Sync copy. In case the storage accounts are in VNET, you will have to keep this value to false. For server copy you can find more details [here](https://blogs.msdn.microsoft.com/windowsazurestorage/2012/06/12/introducing-asynchronous-cross-account-copy-blob/)| 	
|**Serilog : connectionString**|This is the connection string of the storage account where the detailed diagnostic logs will be generated.| 	

* ##### restore.utility	
This utility is responsible for restoring the incremental backup. Before the incremental backup re-store, user will have to creates a new storage account manually where the data needs to be restored. User will also have to first move the full back up using AZCopy to the destination i.e. newly created storage account.
User will have to initiate the restore process manually by giving the start date and end date for which data needs to be restored.
For Example: Re-store process reads the data from the table storage for the period 01/08/2018 to 01/10/2018 sequentially to perform the re-store. The date format is mm/dd/yyyy

**Connection Strings**

| Key Name        | Description           |
| ------------- |:-------------| 
| **BackupTableStorage**      | This the connection string where you have created the table to store the event metadata. This should be same as “BackupTableStorage” from storage utility. |
| **BackupBlobStorage**      | This is the connection string where the listener is keeping the backup. This should be same as “BackupBlobStorage” from storage utility. |	
| **RestoreBlobStorage**      | This is the connection string of a storage account where the restore will be performed. Please note you will have to first bring the full backup from the “fkbkp” folder using the AzCopy. |	

**App Settings**

| Key Name        | Description           |
| ------------- |:-------------| 
| **BackupTableName**      | Name of the Azure table name where the event metadata has been stored. This should be same as “BackupTableName” from storage utility. |
| BlobSASExpiryInMts      | SAS Key Expiry in Minutes. This is used in case of Server copy.| 
| IsServerCopy      | To perform server copy or Sync copy. In case the storage accounts are in VNET, you will have to keep this value to false. For server copy you can find more details here. |
| **Serilog : connectionString**      | This is the connection string of the storage account where the detailed diagnostic logs will be generated.|

## Resources
- https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-event-overview
- https://docs.microsoft.com/en-us/azure/cosmos-db/table-storage-overview

