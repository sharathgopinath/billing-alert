import { IConstruct } from '@aws-cdk/core';

export default {
    getAccount(construct: IConstruct): string {
        return construct.node.tryGetContext("Account");
    },

    getAppName(construct: IConstruct): string {
        return construct.node.tryGetContext("AppName");
    },

    getDepAssetsS3Bucket(construct: IConstruct): string {
        return construct.node.tryGetContext("DepAssetsS3Bucket");
    },

    getLambdaPackageName(construct: IConstruct): string {
        return construct.node.tryGetContext("LambdaPackageName");
    }
};