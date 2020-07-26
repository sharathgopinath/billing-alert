#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from '@aws-cdk/core';
import context from '../helpers/context';
import { ConsumerAppStack } from '../lib/consumer-app-stack';
import { PersistenceStack } from '../lib/persistence-stack';
import { DepAssetsStack } from '../lib/dep-assets-stack';

const app = new cdk.App();
const consumerAppStackName = `${context.getAppName(app)}-consumer-stack`;
const persistenceStackName = `${context.getAppName(app)}-persistence-stack`;
const depAssetsStackName = `${context.getAppName(app)}-dep-assets-stack`;

var depAssetsStack = new DepAssetsStack(app, 'dep-assets-stack', {
        S3BucketName: context.getDepAssetsS3Bucket(app)
    }, {
    stackName: depAssetsStackName,
    description: 'Billing Alert deployment assets stack',
    tags:{
        'AppName': context.getAppName(app),
        'StackName': depAssetsStackName
    }
});

var persistenceStack = new PersistenceStack(app, 'persistence-stack', {
    stackName: persistenceStackName,
    description: 'Billing Alert persistence stack',
    tags:{
        'AppName': context.getAppName(app),
        'StackName': persistenceStackName
    }
});

new ConsumerAppStack(app, 'consumer-app-stack', {
        billingAlertStoreTableArn: persistenceStack.dynamoDb.table.tableArn,
        billingAlertStoreTableName:persistenceStack.dynamoDb.table.tableName,
        s3Bucket: depAssetsStack.bucket,
        lambdaPackageName: context.getLambdaPackageName(app)
    }, 
    {
        stackName: consumerAppStackName,
        description: 'Billing Alert application stack',
        tags:{
            'AppName': context.getAppName(app),
            'StackName': consumerAppStackName
        }
    });
