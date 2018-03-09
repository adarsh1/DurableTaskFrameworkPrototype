# Read Me

This Project is a simple Prototype of the [Durable Task Framework](https://github.com/Azure/durabletask "Durable Task Framework") using its Azure Storage based orchestrations.

## Instructions
Provide a link to a Azure Storage Account in ServiceSettings.cs in The DurableTaskFrameworkPrototype Project.

Build the Solution using Visual Studio.

in the bin folder run Server.exe run one/multiple instances of Client.exe

## Behavior
Enter  any alphabetical string in the Clients and an orchestration will be queued which will output a running count of the total usage of each letter on the Server window.