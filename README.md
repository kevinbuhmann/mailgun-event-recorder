# mailgun-event-recorder
This application will continually read events from Mailgun as they occur and will forward all events to an Event Hub. In case of failures or delays the app will pick back up after its last checkpoint.

## Dependencies

### Mailgun Account
This is where the events will be read.

### Azure Storage Account
This is where the checkpoint will be stored.

### Event Hub
This is where the events will be sent.

## Settings
Settings can be configured in the appsettings.json file or with environment variables.

### General Settings

#### CheckpointPeriodInSeconds
A checkpoint will be taken after the app processes Mailgun Events spanning this length of time. The longer the checkpoint, the more efficient the app will perform. However, if the app experiences a failure, it will have to go back to its previous completed checkpoint which could result in duplicate events being sent to Event Hub.

#### TimerPeriodInSeconds
After each checkpoint, the app will sleep for this amount.

#### MailgunApiKey
Api key for mailgun account.

#### MailgunSendingDomains
A comma-separated list of all sending domains that you wish to record events for.

#### MailgunAccountEventStoragePeriodInDays
The number of days that your Mailgun account stores events. The app will attempt to resume where it left off at its last checkpoint, but will never try to get events older than this period.

#### MailgunEventLagTimeInSeconds
Mailgun events may not show up in your account instantly. This is the lag time the app should use when recording events. Set this to the time it takes Mailgun events to be available in your account logs after they occur.

#### AzureStorageConnectionString
Connection string for azure storage account.

#### EventHubConnectionString
Connection string (including EntityPath) for event hub account.