# Payment Gateway API

The Payment Gateway requires a bank service to be running on http://localhost:8080/.
The Payment Gateway REST API has two endpoints: 

### ProcessPayment: 
Allows the user to process a payment by sending it to the bank.

Upon processing a payment, a unique ID will be returned to the user, alongside the details of the payment attempt.

### GetPayment: 

Allows the user to retrieve details of an already existing payment, by its ID.

## Design Decisions

- The API follows the REST pattern and uses HTTP Status Codes to return information back to the user.
- Any errors in the API are always caught at the controller level and sanitised to ensure that internal implementation details are not exposed.
- The program performs logging throughout to provide useful debugging/monitoring information.
- The API has separate models for Web and Internal DTOs, to ensure that only the required data is used where needed.
- The API does not persist the full Card Number and CVV of any requests, as these are sensitive items of data and would pose a security risk if stored.
- The API does store the authorisation code returned from the bank service, as this is not sensitive information and can be useful for auditing payments.
- When the API calls to the external bank service, it implements a retry strategy with backoff, to account for the external server being overloaded or malfunctioning.
- The API has been split into logic layers: 
  - Controller as the entry point of the application
  - API for performing validation and business logic
  - Repository for database persistence
- The Repository layer is currently in memory only, but could easily be swapped out for a real database connection.
- The API currently supports payments up to a maximum value of 2,147,483,647 in the minor currency unit. This could be changed in the future if larger payments are required.
- The API interacts with the external bank service over HTTP. Ideally this would be HTTPS, as sensitive information is being transmitted.


## Testing
The API has implements testing at a number of levels

- Unit: Testing the individual classes in isolation, without any external connections. 
- Integration: Testing the various layers together.
- End To End: Testing the entire workflow with a running version of the application and an external bank service.

