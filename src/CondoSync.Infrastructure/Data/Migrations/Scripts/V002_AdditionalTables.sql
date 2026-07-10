-- ============================================
-- CondoSync v2.0 - Migration V002
-- Tabelas adicionais
-- ============================================

-- Tabela: documents
CREATE TABLE public.documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    uploaded_by UUID NOT NULL REFERENCES public.users(id),
    name VARCHAR(300) NOT NULL,
    description TEXT,
    document_type VARCHAR(50) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    content_type VARCHAR(50) NOT NULL,
    file_size INTEGER NOT NULL,
    version INTEGER DEFAULT 1,
    previous_version_id UUID REFERENCES public.documents(id),
    visibility VARCHAR(30) DEFAULT 'all',
    document_date DATE,
    expires_at DATE,
    is_active BOOLEAN DEFAULT true,
    requires_signature BOOLEAN DEFAULT false,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_documents_condo_type ON public.documents(condominium_id, document_type);
CREATE INDEX idx_documents_expires ON public.documents(expires_at) WHERE expires_at IS NOT NULL;

-- Tabela: notifications
CREATE TABLE public.notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    user_id UUID NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    title VARCHAR(300) NOT NULL,
    body TEXT,
    type VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50),
    entity_id UUID,
    action VARCHAR(50),
    channels JSONB DEFAULT '["in_app"]',
    sent_at TIMESTAMPTZ,
    read_at TIMESTAMPTZ,
    is_read BOOLEAN DEFAULT false,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_notifications_user_read ON public.notifications(user_id, is_read, created_at DESC);
CREATE INDEX idx_notifications_condo_unread ON public.notifications(condominium_id, user_id) WHERE is_read = false;

-- Tabela: activity_logs
CREATE TABLE public.activity_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    user_id UUID REFERENCES public.users(id),
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id UUID,
    old_values JSONB,
    new_values JSONB,
    details JSONB,
    ip_address VARCHAR(50),
    user_agent TEXT,
    user_role VARCHAR(30),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_activity_logs_condo ON public.activity_logs(condominium_id, created_at DESC);
CREATE INDEX idx_activity_logs_entity ON public.activity_logs(entity_type, entity_id);
CREATE INDEX idx_activity_logs_user ON public.activity_logs(user_id, created_at DESC);

-- Tabela: guest_lists
CREATE TABLE public.guest_lists (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    created_by UUID NOT NULL REFERENCES public.users(id),
    booking_id UUID REFERENCES public.bookings(id),
    unit_id UUID REFERENCES public.units(id),
    title VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    event_date DATE NOT NULL,
    start_time TIME,
    end_time TIME,
    max_guests INTEGER DEFAULT 50,
    requires_qr_code BOOLEAN DEFAULT true,
    status VARCHAR(20) DEFAULT 'Active',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_guest_lists_condo ON public.guest_lists(condominium_id);
CREATE INDEX idx_guest_lists_date ON public.guest_lists(event_date);
CREATE INDEX idx_guest_lists_booking ON public.guest_lists(booking_id);

-- Tabela: outbox_messages
CREATE TABLE public.outbox_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type VARCHAR(500) NOT NULL,
    content JSONB NOT NULL,
    headers JSONB,
    status VARCHAR(20) DEFAULT 'pending',
    retry_count INTEGER DEFAULT 0,
    max_retries INTEGER DEFAULT 5,
    last_error TEXT,
    error_stack_trace TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    processed_at TIMESTAMPTZ,
    sent_at TIMESTAMPTZ
);

CREATE INDEX idx_outbox_pending ON public.outbox_messages(status, created_at) WHERE status = 'pending';

-- Tabela: event_store
CREATE TABLE public.event_store (
    id BIGSERIAL PRIMARY KEY,
    aggregate_type VARCHAR(100) NOT NULL,
    aggregate_id UUID NOT NULL,
    version INTEGER NOT NULL,
    event_type VARCHAR(500) NOT NULL,
    event_data JSONB NOT NULL,
    metadata JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(aggregate_type, aggregate_id, version)
);

CREATE INDEX idx_event_store_created ON public.event_store(created_at);

-- Tabela: polls
CREATE TABLE public.polls (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    condominium_id UUID NOT NULL REFERENCES public.condominiums(id),
    created_by UUID NOT NULL REFERENCES public.users(id),
    title VARCHAR(300) NOT NULL,
    description TEXT,
    poll_type VARCHAR(30) DEFAULT 'Single',
    is_anonymous BOOLEAN DEFAULT false,
    is_mandatory BOOLEAN DEFAULT false,
    requires_unit_vote BOOLEAN DEFAULT false,
    voting_rule VARCHAR(50),
    is_binding BOOLEAN DEFAULT false,
    elected_candidate_id UUID,
    approved_option_id UUID,
    voter_slug VARCHAR(100),
    options JSONB NOT NULL,
    starts_at TIMESTAMPTZ NOT NULL,
    ends_at TIMESTAMPTZ NOT NULL,
    total_votes INTEGER DEFAULT 0,
    results_visibility VARCHAR(30) DEFAULT 'after_end',
    status VARCHAR(30) DEFAULT 'Draft',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_polls_condo_status ON public.polls(condominium_id, status);

-- Tabela: poll_votes
CREATE TABLE public.poll_votes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    poll_id UUID NOT NULL REFERENCES public.polls(id) ON DELETE CASCADE,
    unit_id UUID REFERENCES public.units(id),
    resident_id UUID REFERENCES public.residents(id),
    user_id UUID REFERENCES public.users(id),
    selected_options UUID[] NOT NULL,
    voted_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_poll_votes_poll ON public.poll_votes(poll_id);
