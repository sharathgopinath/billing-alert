#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from '@aws-cdk/core';
import { BillingAlertAppStack } from '../lib/billing-alert-app-stack';
import context from '../helpers/context';

const app = new cdk.App();
const stackName = `${context.getAppName(app)}-stack`;
new BillingAlertAppStack(app, 'BillingAlertAppStack', {
    stackName: stackName,
    description: 'Billing Alert Stack',
    tags:{
        'AppName': context.getAppName(app),
        'StackName': stackName
    }
});
