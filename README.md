# Blockcore DNS

Domain Name System Server that utilizes Decentralized identifiers (DIDs) for updates

## Introduction

The help mitigate infrastructure attacks, such as shutdown of cloud-hosted services, we allow anyone to host and run our software. This will make it harder to have any efficient attacks against our networks.

The Blockcore DNS is a type of Dynamic DNS ([DDNS](https://en.wikipedia.org/wiki/Dynamic_DNS)) that allows a semi-trust or no-trust setups where anyone that runs Blockcore software can become part of the ecosystem.

Anyone can run the Blockcore DNS software for their own domain, and can allow anyone or approved-list of hosters to be part of their official DNS.

## DNS Server

The DNS server has several responsibilities

- Resolve DNS A/AAA queries for domains that are registered to it.
- API to register IP address/domains (DDNS) of Blockcore hosted services.
- API to get list of IP address/domains of Blockcore hosted services.
- Periodically check that Blockcore hosted services are online (otherwise delist them).

## API

The running instance of Blockcore DNS will have a REST API that allows running instances of the Blockcore software (e.g. Blockcore Indexer, Blockcore Vault) to announce their current public IP address where consumers can access the public APIs being hosted by individual third parties.

## Agent

Agent mode is an instance of Blockcore DNS run by individuals hosting Blockcore software (e.g. Blockcore Indexer, Blockcore Vault), it runs side by side such software and can resolve and announce their current public IP address to Blockcore DNS servers.

## How it works

- Actor A will buy a domain, e.g. "myservers.com".
- Actor A runs the Blockcore DNS software, preferably using Docker.
- The Blockcore DNS will be configured by Actor A and setup with an DID (Decentralized Identfier).
- The Blockcore DNS will automatically generate and configure an TLS certificate for secure endpoint communication. It is not a requirement to use certificates, as all messages will be signed.
- Actor A configure their DNS instance to either be fully public (will be open for various attacks) or pre-approved list of DIDs. (fully public DNS is not supported at this stage)

Next step is for Actor B to run, e.g. Blockcore Indexer or Blockcore Vault.

- Actor B will install and run the Blockcore software.
- Actor B will install and run the Blockcore DNS in agent mode.
- Actor B will be able to configure Blockcore DNS which domains they want their hosted software to be a member of, e.g. "myservers.com".

Final step is consumers (users, etc.) that need to consume blockchain and other data.

- Actor C will install the Blockcore Extension, which is an lightweight wallet that depends on hosted nodes.
- Actor C will either choose "Default", from a list of pre-approved domains, or custom domain input for where they want to connect.
- Actor C software will perform a normal DNS lookup and retrieve the IP of all running instances, and pick one IP to communicate with.

## Existing DNS features

The Blockcore node supports DNS-mode, where it will return IP-addresses of public nodes that are connected to the node. This code is based upon this DNS library:

https://github.com/kapetan/dns

We should consider using the same library for Blockcore DNS. It provided DNS logic for client and server logic.
