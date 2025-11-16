rem

' RETRIEVE array_name, NEW|APPEND, number|*, SQL Data retrieval statement
retrieve Cust, APPEND, *, "select CustomerID, Name, Email,City from Customers where CustomerID=1"  
retrieve Cust, APPEND, 1, "select CustomerID, Name, Email,City from Customers"  
retrieve Cust, APPEND, 2, "select CustomerID, Name, Email,City from Customers"  

print "Count="+ubound("Cust")
print [Cust.CustomerID]+": Name:"+[Cust.Name]+", Email: "+[Cust.Email]



halt

