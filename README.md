# TheMicroServices

Demonastrate Event Driven Microservice based architecture.


## Domain:

Borrowed 3 Boundned context from ERP:

1. **Purchase**: User can purchase Item. Purchase department store that purchased item information.
2. **Sales**: After saling items, user can store sales information
3. **Inventory**: When any purchase or Sales occured, Stock will be automatically updated.


## Architecture:

System is consist of 3 Microservices:

* **Purchase Microservice**: This microservice is responsible to manage Purchase related informtion and raised an event after purchase done. Based on the event,
consumer will be updated accordingly.

* **Sales Microservice**: This microservice is responsible to manage Sales related information. When any sales happend it will raised an event so that its interested
party can update and act accordingly.

* **Inventory Microservice**: This microservice is responsible to manage Products and also when any purchase or sales happended it will update stock based on their raised events.

* **CQRS**: CQRS is implemented inside Microservices.

* **Nothing shared architecture**. Every Microservice has its own database.


Here, I have shouwn 3 ways to communicate Microservice to Microservice communication:

* **Event Driven**: Any microservice Publish events to the Message Queue. Other Microservices subscribe those events and act accordingly.
* **REST**: Not recommended approach. But showing here as an example. One Microservice Directly call other Micrservice to the REST way.
* **GRPC**: Recommended approach for now a days. It is very similar like old Remote Procedure Call(RPC).  


#### High Level Design:

![High Level](https://github.com/habibsql/TheMicroservices/blob/main/Docs/highlevel.JPG?raw=true)

#### Command/Query Segrigation:

* In CQRS command and query are two distinct parts. Here Read Model and Write Model are different.
* Command execution change the application state. So command execution flows are risky by nature and very carefully need to implement.
* Query execution does not change anything just read the data. So little relux but if application contain sensitive data than
  it might be risky too. 

![CommandQuery](https://github.com/habibsql/TheMicroservices/blob/main/Docs/cq.JPG?raw=true)


## Event Driven Architecture:

It is difference than traditional Request driven model. In event driven architecture one system emit events, other systems
capture those events and react accordingly. The system communicate, processing based on event flows. Someone called
it as Publisher subscriber Model.

![Event-Driven](https://github.com/habibsql/TheMicroservices/blob/main/Docs/ed.JPG?raw=true)

## CAP theorem:

It is also called brewer's theorem. It says whether any distributed system either achieve at most 2 out of 3 (It is actually proved).By cleaverly design so many systems in real world, if has achieved all 3 based on some tricks or you can say made balanced between them. It is business decission rather technical decission.

#### 3 Parts are:
* **C**onsistency: All clients must see the same data from all differnce places. 
* **A**vailability: When any client requests for data, he/she can get data immediately, event 1 or 2 or mnore nodes are down. 
* **P**artitioning: ommunication break within a distributed system.

![CAP](https://github.com/habibsql/TheMicroservices/blob/main/Docs/cap.JPG?raw=true)

## Eventual Consistency:

It name described data is replicated other sources gradually. It is weekest consistency but many benifits, one of is easy scaling & best performace. For that reason many distributed system
use this and sacrifice strong consistency. Its sequence is:
  **1**. Write node-1 from client
  **2**. Acknoledge to client from node-1
  **3**. Eventual write propagates other nodes via cluster.
 
![CAP](https://github.com/habibsql/TheMicroservices/blob/main/Docs/ec.JPG?raw=true)

## Circuit Breaker/Retry Policy

Circuit breaker and retry policy are two important things in Microservice world. Here It assume that network is unreliable and may lost its 
connection any time. It is a major issue here. So solve this issue and build a system resilece 2 patters play important role.

* **Retry Policy**: Allow how many times (after time interval) a service call. 

* **Circuit Breaker Policy**: Idea is very simple. We should wrap any function with Circuit breaker object and circuit breaker object monitor its failure.
When failure happend at cirtain time interval, circuit breaker trips all futher call with a error message. It has 3 states: 

* **i)Open**: Returns an error for calls without executing functions.
* **ii)Closed**: Call all the services.
* **iii)Half-Open**: After timeperiod, any issue raised then check underlying problem still exists or not. Any single call failed half CB changed to half open state.

![Circuit-Breaker](https://github.com/habibsql/TheMicroservices/blob/main/Docs/cb.JPG?raw=true)

## Technology Used:

* C# Language
* ASP.NET Core Framework.
* RabbitMQ: Message Broker for event pub/sub feature.
* MongoDB: NoSQL database (Prefefer NoSQL instead of SQL for better schemaless/scalability/speed etc.
* Rest/GRPC: Service-to-service communication.
* Polly: A Nuget package for setting Auto Retry Remote call settings.
* TestHost: (Microsoft.AspNetCore.TestHost) for WebAPI integration test.

## Demonastration:

* Synchronous Event Driven with Service Broker
* CQRS
* NOSQL
* Service to Service communication with REST with Retry Policy
* Service to Service communication with GRPC with Circuit Breaker Policy
* Web API service Integration Test
