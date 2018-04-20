DROP PROCEDURE IF EXISTS %SCHEMA%.uspDeleteIncomingEnvelopes;
DROP PROCEDURE IF EXISTS %SCHEMA%.uspDeleteOutgoingEnvelopes;
DROP PROCEDURE IF EXISTS %SCHEMA%.uspDiscardAndReassignOutgoing;

IF OBJECT_ID('%SCHEMA%.jasper_outgoing_envelopes', 'U') IS NOT NULL
  drop table %SCHEMA%.jasper_outgoing_envelopes;


IF OBJECT_ID('%SCHEMA%.jasper_incoming_envelopes', 'U') IS NOT NULL
  drop table %SCHEMA%.jasper_incoming_envelopes;

IF OBJECT_ID('%SCHEMA%.jasper_dead_letters', 'U') IS NOT NULL
  drop table %SCHEMA%.jasper_dead_letters;

drop type IF EXISTS %SCHEMA%.EnvelopeIdList;