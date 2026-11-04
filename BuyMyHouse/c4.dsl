workspace {

    !identifiers hierarchical

    model {
        u = person "User"
        ss = softwareSystem "BuyMyHouse" {
            api = container "API"
            cdp = container "Customer Data Processor"
            email = container "Emailing Service"
        }

        as = softwareSystem "Azure Storage" {
            blob = container "Azure Blob Storage"
            table = container "Azure Table Storage"
            queue = cotainer "Azure Queue Storage"
        }

        u -> ss.api "Uses"
        ss.api -> as "Reads from and writes to"
                
    }

    views {
        systemContext ss "Diagram1" {
            include *
        }

        container ss "Diagram2" {
            include *
        }
    }

}