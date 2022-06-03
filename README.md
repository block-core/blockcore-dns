# Blockcore DNS

Domain Name System Server that utilizes Decentralized Identifiers (DIDs) for updates

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

- Actor B will buy a domain or use a subdomain (by convention we use `coin.service.domain.com` i.e `btc.indexer.blockcore.net`)
- Actor B will configure the domain/subdomain to resolve an NS record to the Actor A server "myservers.com". or one of Blockcore-DNS servers (i.e `ns.blockcore.net`)
- Actor B will install and run the Blockcore software.
- Actor B will install and run the Blockcore DNS in agent mode.
- Actor B will be able to configure Blockcore DNS which domains they want their hosted software to be a member of, e.g. "myservers.com".

Final step is consumers (users, etc.) that need to consume blockchain and other data.

- Actor C will install the Blockcore Extension, which is an lightweight wallet that depends on hosted nodes.
- Actor C will either choose "Default", from a list of pre-approved domains, or custom domain input for where they want to connect.
- Actor C software will perform a normal DNS lookup and retrieve the IP of all running instances, and pick one IP to communicate with.

## Getting Started

Use the command `--dns-help` to show the available command line options

```
dotnet run --dns-help

Domain Name System Server that utilizes Decentralized Identifiers(DIDs) for updates.
See the application options below.

options:

--did            mode to generate a did key pair
--agent          mode to run as client that can register domains/IPs to a dns server (ddns)
[unspecified]    otherwise to run as a dns server mode that can serve A/AAAA records and allow agents to register domains and IPs
```

### How to create a Decentralized Identifier (DID)

```
dotnet run --did

Secret key add to config 64693d03c3bf79dd1a47ba475db2ed7cd22656c117f1d9329b5bb2324585e3b2
Public identity did:is:03842ed7d440c0ab437f48db8bffbffdfca307253a3c38a444f4fb91297db1e45d
```

The secret and DID will be used later in the agent and dns `appsettings.config` files

### How to run an agent

```
dotnet run --agent
```

When running in agent mode use the secret generated from the previous step and provide the DID of that secret to any dns server that you want the agent to register it's domain data with.

The agent instance will periodically discover the current IP address, and broadcast each configured host to register it's domain with a DnsHost (DNS server)
The agent will build a request and sign it using schnorr signatures before sending to the server.

Here is a config example of an agent configured to register a Bitcoin indexer with two dns servers 

```
 "DnsAgent": {
    "Hosts": [
      {
        "DnsHost": "http://dns1-domain.com",
        "Domain": "btc.indexer.agent-domain.com",
        "Port": "9910",
        "Symbol": "BTC",
        "Service": "Indexer"
      },
      {
        "DnsHost": "http://dns2-domain.com",
        "Domain": "btc.indexer.agent-domain.com",
        "Port": "9910",
        "Symbol": "BTC",
        "Service": "Indexer"
      }
    ],
    "IntervalMinutes": "5",
    "DnsttlMinutes": 20,
    "Secret": "64693d03c3bf79dd1a47ba475db2ed7cd22656c117f1d9329b5bb2324585e3b2"
  }
```

### How to run a DNS server

```
dotnet run
```

Here is a config exmpale of an configuration to listen to two DID 

```
"Dns": {
    "ListenPort": "53",
    "IntervalMinutes": "5",
    "EndServerIp": "192.168.1.1",
    "Identities": [
      "did:is:03ba00574cc3821ae3d5f367696692e1b20ea25e70565e6fa6a07e7f74d266aa39",
      "did:is:03842ed7d440c0ab437f48db8bffbffdfca307253a3c38a444f4fb91297db1e45d"
    ]
  },
```

The DNS server exposes the following endpoints 

```
POST /api/dns/addEntry   - add a new domain entry
GET  /api/dns/entries    - get the list of DNS A/AAAA records
GET  /api/dns/services   - get the list of domains registered
GET  /api/dns/ipaddress  - return the callers ip address
```

Registration requests that have a domain and IP address will also register an A/AAAA recored to resolve the domain to that IP address (effectively acting as a Dynamic DNS)

#### Open port 53 linux
When setting up a DNS server on linux its required to open port 53
see this guid to open that port or follow the steps bellow
https://www.linuxuprising.com/2020/07/ubuntu-how-to-free-up-port-53-used-by.html

Open this file
```
nano /etc/systemd/resolved.conf
```

Set this params
```
DNS=1.1.1.1
DNSStubListener=no
```

Create a link
```
ln -sf /run/systemd/resolve/resolv.conf /etc/resolv.conf
```

Reboot


## External dependencies

The DNS server uses the following nuget package to handle DNS queries

https://github.com/kapetan/dns

Docs and a usefull tool to help debug DNS queries (used by letsencrypt)

https://unboundtest.com/
https://letsencrypt.org/docs/caa/

