export default{
    getCode(streamNamePrefix: string){
        return `
        -- STREAM (in-application): a continuously updated entity that you can SELECT from and INSERT into like a TABLE
        CREATE OR REPLACE STREAM "DESTINATION_SQL_STREAM" (customerId INT, tollAmount INT);
        
        -- Create pump to insert into output 
        CREATE OR REPLACE PUMP "STREAM_PUMP" AS INSERT INTO "DESTINATION_SQL_STREAM"
        -- Select all columns from source stream
        SELECT STREAM "customerId", SUM("tollAmount")
        FROM "${streamNamePrefix}_001"
        GROUP BY "customerId", STEP("${streamNamePrefix}_001".ROWTIME BY INTERVAL '60' SECOND)
        `;
    }
}