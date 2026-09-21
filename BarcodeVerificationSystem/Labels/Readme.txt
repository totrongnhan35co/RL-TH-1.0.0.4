* Version 1.0.0 (July 28, 2023) | Build 230728 1154

* Version 1.0.1 (August 15, 2023) | Build 230815 1510
- Added an export log function

* Version 1.0.2 (August 21, 2023) | Build 230821 1650
- Added reset and synchronization of the display numbers for Sent and Received data when stopping the process

* Version 1.0.3 (August 31, 2023) | Build 230831 1045
- Fixed the checked result backup error when the job's total check is equal to or greater than 1000
- Added automatic adjustment of the storage structure when opening a job in case of a backup error

* Version 1.0.4 (November 10, 2023) | Build 231110 1321
- Improved UI update performance during verification
- Improved the verification speed
- Improved search speed in preview data ui and checked result ui
- Removed "Added automatic adjustment of the storage structure when opening a job in case of a backup error"
- Fixed backup error when data contains commas
- Fixed update printing status error when data compare format contain text field
- Fixed update printing status error when on Verify and print mode
- Added prevention of flicker when updating the printed status and checked results
- Added check duplicate data in database base on compare data format
- Increased the limit of Delay sensor and Disable sensor

* Version 1.0.5 (November 29, 2023) | Build 231129 1649
- Fixed export log error in certain cases.
- Added highlighting of duplicate data in the database preview interface

* Version 1.0.6 (December 05, 2023) | Build 231205 1618
- Modified date time format name of backup file
- Added a reprint confirmation box

* Version 1.0.7.0 (October 14, 2024) | Build 241014 1550 
- Fixed an issue where the default account could be deleted
- Fixed an issue where no notification appeared when the Web API for retrieving setting parameters failed to load.
- Added Cognex In-Sight Vision Series OCR Camera
- Added functionality for reading multiple barcodes 
- Added wait time before stopping
- Added 2 sensor control parameter settings (customer's option)
- Fixed export error with (.csv) files
- Modified the cleared sensor controller history command to prevent repeated sending
- Added functionality for retrieving camera data via TCP/IP (excluding image data), use for Multi-Sync read
- Added RSFP command log for feedback on finished printed pages from the printer
- Added a script for the Cognex camera to always push data to R-Link, even if the code cannot be read
- Added Basic Read and Multi-Read option in camera settings
- Modified to not clear the printer buffer POD when starting a new job (without using the CLPB command)
- Added log sending and RSFP log (formatted time column), and fixed file override 
- Added disposal of resources for reconnecting Dataman cameras
- Improved the speed of the POD function by using a queue for sending 
- Added a check for the In-Sight Vision camera's online status before starting
- Added a duplicate status to the checked result export function
- Added GAP length and error length settings for the sensor controller
- Added Multi-Sync read functionality for In-Sight Vision cameras
- Updated the database log and added a checked log function
- Added a TCP/IP command for triggering camera output on failure
- Added barcode scanner device connection using the serial protocol

* Version 1.0.7.1 (October 18, 2024) | Build 241018 1335
- Fixed issue where the "Check all printer settings" feature was not working
- Fixed issue with the export data feature where headers were incorrect and not all data was included
- Fixed issue where the default camera type was not shown during the first-time software installation
- Fixed issue where multi-read functionality for Dataman cameras was not working
- Modified realease note format
- Added a Restart Application feature

* Version 1.0.7.2 (January 06, 2025) | Build 250106 1435
- Added get sample function
- Added 7 data export conditions
- Added <EOL> condition in camera data

* Version 1.0.7.3 (February 19, 2025) | Build 250221 1635
- Added Split character Selection  in Printer Settings.
- Added Checking both old and new USB license.

* Version 1.0.7.4 (March 15, 2025) | Build 250424 1635
- Updated Position Data Through TCP/IP for Camera In-Sight.
- Compared both Position and Data.
- Added Editing Command and Index after Command.
- Added Export All For One function.

* Version 1.0.7.5 (March 18, 2025) | Build 250318 1530
- Added an Export Waiting Data 1 to Export data function.

* Version 1.0.7.6 (May 12, 2025) | Build 250512 1635
- Updated Position Data Through TCP/IP for Camera In-Sight.
- Compared both Position and Data.
- Added Editing Command and Index after Command.
- Added Export All For One function.
- Added First Row Header Selection.

* Version 1.0.7.7 (May 13, 2025) | Build 250513 1635
- Added Resume Encoder for PLC.
- Added an Delay Output Error parameter

* Version 1.0.7.8 (September 13, 2025) | Build 250913 1324
- Added support for the Keyence CV-X Series camera

* Version 1.0.7.9 (September 13, 2025) | Build 251104 1624
- Added PLC error output delay feature.
- Added feature to customize error output messages and save a history of these messages for re-output.
- Added a setting to allow users to select whether the job deletion feature is visible (show/hide job deletion).
- Implemented automatic job name verification to check for duplicates and display an alert message.
* Version 1.0.8.0 (March 2, 2025) | Build 260302 0916
- Added "Printed" status marking for data post-Camera verification.