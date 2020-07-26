![Deploy to AWS](https://github.com/sharathgopinath/billing-alert/workflows/Deploy%20to%20AWS/badge.svg)

# billing-threshold-alert
Requirement - 2 of the Data solutions on aws kinesis [whitepaper](docs/whitepaper-streaming-data-solutions-on-aws-with-amazon-kinesis.pdf)

<img src="img/Requirement_2.png" width="800">

***Source: https://d0.awsstatic.com/whitepapers/whitepaper-streaming-data-solutions-on-aws-with-amazon-kinesis.pdf***

## TODO
- Documentation
- Infra-as-code for Producer

## Run tests

```
> docker-compose up -d
```

```
> cd ./tests
> dotnet test Consumer.Tests
```
