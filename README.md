# Using Azure Function to update Azure Redis Cache from a file in Azure BLOB storage

## Background 
A common scenario with using distributed cache to support large highly-available applications such as inventory/ordering systems is to keep reference data for speedy access. Often this data may come from third party systems or legacy platforms such as mainframes or older ERPs and exported as CSV or text file.

## About this code 
In this example code, the Azure Function updates the Azure Cache on a trigger from BLOB storage when a file is uploaded. The file is a text file containing comma delimited key and value pair.

## Code Design

Azure Function is triggered by an update to the file in the Blob storage. The code assumes that the file is a CSV file containing unique keys and value associated with the key. The Azure Function reads the BLOB from Azure storage as a stream, uploading a batch of key-value pairs at a time to Redis Cache. The batch size is a configurable parameter in the application settings.

### Limitations
The CSV file must contain unique keys. If the key already exists in the Redis Cache, the value for the key will be updated. However, the key repeats with the same batch, the process will fail.

## Application Configuration



Please feel free to clone and fork. Looking forward to hear any feedback.




