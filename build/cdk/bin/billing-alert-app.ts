#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from '@aws-cdk/core';
import context from '../helpers/context';
import { AppStack } from '../lib/app-stack';
import { PersistenceStack } from '../lib/persistence-stack';

const app = new cdk.App();
const appStackName = `${context.getAppName(app)}-stack`;
const persistenceStackName = `${context.getAppName(app)}-persistence-stack`;

var persistenceStack = new PersistenceStack(app, "BillingAlertPersistenceStack", {
    stackName: persistenceStackName,
    description: 'Billing Alert persistence stack',
    tags:{
        'AppName': context.getAppName(app),
        'StackName': persistenceStackName
    }
});

new AppStack(app, 'BillingAlertAppStack', {
        billingAlertStoreTableArn: persistenceStack.dynamoDb.table.tableArn,
        billingAlertStoreTableName:persistenceStack.dynamoDb.table.tableName,
    }, 
    {
        stackName: appStackName,
        description: 'Billing Alert application stack',
        tags:{
            'AppName': context.getAppName(app),
            'StackName': appStackName
        }
    });
